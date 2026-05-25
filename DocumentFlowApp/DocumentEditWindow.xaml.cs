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
                    string sql = $@"SELECT d.*, dt.TypeName 
                           FROM Documents d
                           JOIN DocumentTypes dt ON d.DocTypeId = dt.Id
                           WHERE d.Id = {_docId}";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        string docType = reader["TypeName"].ToString();
                        if (docType == "Входящий")
                            cmbDocType.SelectedIndex = 0;
                        else if (docType == "Исходящий")
                            cmbDocType.SelectedIndex = 1;

                        txtNumber.Text = reader["DocNumber"].ToString();
                        dpDate.SelectedDate = Convert.ToDateTime(reader["DocDate"]);
                        txtTime.Text = reader["DocTime"].ToString();
                        txtCounterparty.Text = reader["Counterparty"].ToString();
                        cmbEmployee.SelectedValue = reader["EmployeeId"];
                        txtSubject.Text = reader["Subject"].ToString();
                        txtContent.Text = reader["Content"].ToString();
                    }
                    reader.Close();
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
                int docTypeId = docType == "Входящий" ? 1 : 2;

                if (string.IsNullOrEmpty(docType) || string.IsNullOrEmpty(txtNumber.Text) ||
                    dpDate.SelectedDate == null || string.IsNullOrEmpty(txtCounterparty.Text) ||
                    cmbEmployee.SelectedValue == null)
                {
                    MessageBox.Show("Заполните все обязательные поля");
                    return;
                }

                string subject = txtSubject.Text.Replace("'", "''");
                string content = txtContent.Text.Replace("'", "''");
                string counterparty = txtCounterparty.Text.Replace("'", "''");
                string docNumber = txtNumber.Text.Replace("'", "''");

                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql;
                    if (_docId.HasValue)
                    {
                        sql = $@"UPDATE Documents SET 
                        DocTypeId = {docTypeId},
                        DocNumber = N'{docNumber}',
                        DocDate = '{dpDate.SelectedDate.Value:yyyy-MM-dd}',
                        DocTime = '{txtTime.Text}',
                        Counterparty = N'{counterparty}',
                        EmployeeId = {cmbEmployee.SelectedValue},
                        Subject = N'{subject}',
                        Content = N'{content}'
                        WHERE Id = {_docId}";
                    }
                    else
                    {
                        sql = $@"INSERT INTO Documents (DocTypeId, DocNumber, DocDate, DocTime, Counterparty, EmployeeId, Subject, Content)
                        VALUES ({docTypeId}, N'{docNumber}', '{dpDate.SelectedDate.Value:yyyy-MM-dd}', 
                                '{txtTime.Text}', N'{counterparty}', {cmbEmployee.SelectedValue}, 
                                N'{subject}', N'{content}')";
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