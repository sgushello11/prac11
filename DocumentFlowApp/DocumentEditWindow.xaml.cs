using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace DocumentFlowApp
{
    public partial class DocumentEditWindow : Window
    {
        private int? _docId;
        private int _currentEmployeeId;

        public DocumentEditWindow(int? docId, int currentEmployeeId)
        {
            InitializeComponent();
            _docId = docId;
            _currentEmployeeId = currentEmployeeId;
            LoadEmployees();
            if (docId.HasValue)
                LoadDocument();
        }

        private void LoadEmployees()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT Id, FullName FROM Employees ORDER BY FullName";
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    cmbEmployee.ItemsSource = dt.DefaultView;
                    cmbEmployee.SelectedValuePath = "Id";
                    cmbEmployee.DisplayMemberPath = "FullName";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки сотрудников: " + ex.Message);
            }
        }

        private void LoadDocument()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = $"SELECT * FROM Documents WHERE Id = {_docId}";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        cmbDocType.Text = reader["DocType"].ToString();
                        txtNumber.Text = reader["DocNumber"].ToString();
                        dpDate.SelectedDate = Convert.ToDateTime(reader["DocDate"]);
                        txtTime.Text = reader["DocTime"].ToString();
                        txtCounterparty.Text = reader["Counterparty"].ToString();
                        cmbEmployee.SelectedValue = reader["EmployeeId"];
                        txtSubject.Text = reader["Subject"].ToString();
                        txtContent.Text = reader["Content"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки документа: " + ex.Message);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string docType = ((System.Windows.Controls.ComboBoxItem)cmbDocType.SelectedItem)?.Content.ToString();
                if (string.IsNullOrEmpty(docType) || string.IsNullOrEmpty(txtNumber.Text) ||
                    dpDate.SelectedDate == null || string.IsNullOrEmpty(txtCounterparty.Text) ||
                    cmbEmployee.SelectedValue == null)
                {
                    MessageBox.Show("Заполните все обязательные поля");
                    return;
                }

                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql;
                    if (_docId.HasValue)
                    {
                        sql = $@"UPDATE Documents SET 
                                DocType = '{docType}',
                                DocNumber = '{txtNumber.Text}',
                                DocDate = '{dpDate.SelectedDate.Value:yyyy-MM-dd}',
                                DocTime = '{txtTime.Text}',
                                Counterparty = '{txtCounterparty.Text}',
                                EmployeeId = {cmbEmployee.SelectedValue},
                                Subject = '{txtSubject.Text}',
                                Content = '{txtContent.Text}'
                                WHERE Id = {_docId}";
                    }
                    else
                    {
                        sql = $@"INSERT INTO Documents (DocType, DocNumber, DocDate, DocTime, Counterparty, EmployeeId, Subject, Content)
                                VALUES ('{docType}', '{txtNumber.Text}', '{dpDate.SelectedDate.Value:yyyy-MM-dd}', 
                                        '{txtTime.Text}', '{txtCounterparty.Text}', {cmbEmployee.SelectedValue}, 
                                        '{txtSubject.Text}', '{txtContent.Text}')";
                    }
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}