using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace DocumentFlowApp
{
    public partial class MainWindow : Window
    {
        private string _userRole;
        private int _userId;
        private int _employeeId;
        private string _userLogin;

        public MainWindow(string role, int userId, int employeeId, string login)
        {
            InitializeComponent();
            _userRole = role;
            _userId = userId;
            _employeeId = employeeId;
            _userLogin = login;

            tbUser.Text = login;
            tbRole.Text = role;

            if (role == "Admin")
            {
                btnAdd.IsEnabled = true;
                btnEdit.IsEnabled = true;
                btnDelete.IsEnabled = true;
            }
            else if (role == "Operator")
            {
                btnAdd.IsEnabled = true;
                btnEdit.IsEnabled = true;
                btnDelete.IsEnabled = false;
            }
            else
            {
                btnAdd.IsEnabled = false;
                btnEdit.IsEnabled = false;
                btnDelete.IsEnabled = false;
            }

            LoadDocuments();
        }

        private void LoadDocuments()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"SELECT d.Id, 
                                          dt.TypeName as DocType,
                                          d.DocNumber,
                                          d.DocDate,
                                          d.DocTime,
                                          d.Counterparty,
                                          e.FullName as EmployeeName,
                                          d.Subject
                                   FROM Documents d
                                   JOIN DocumentTypes dt ON d.DocTypeId = dt.Id
                                   JOIN Employees e ON d.EmployeeId = e.Id
                                   ORDER BY d.Id";
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgDocuments.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string searchNumber = txtSearchNumber.Text.Trim();
            if (string.IsNullOrEmpty(searchNumber))
            {
                LoadDocuments();
                return;
            }

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = $@"SELECT d.Id, 
                                          dt.TypeName as DocType,
                                          d.DocNumber,
                                          d.DocDate,
                                          d.DocTime,
                                          d.Counterparty,
                                          e.FullName as EmployeeName,
                                          d.Subject
                                   FROM Documents d
                                   JOIN DocumentTypes dt ON d.DocTypeId = dt.Id
                                   JOIN Employees e ON d.EmployeeId = e.Id
                                   WHERE d.DocNumber LIKE '%{searchNumber}%'
                                   ORDER BY d.Id";
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgDocuments.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка поиска: " + ex.Message);
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtSearchNumber.Text = "";
            LoadDocuments();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            DocumentEditWindow editWindow = new DocumentEditWindow(null, _employeeId);
            if (editWindow.ShowDialog() == true)
            {
                LoadDocuments();
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgDocuments.SelectedItem == null)
            {
                MessageBox.Show("Выберите документ для редактирования");
                return;
            }

            DataRowView row = (DataRowView)dgDocuments.SelectedItem;
            int docId = Convert.ToInt32(row["Id"]);
            DocumentEditWindow editWindow = new DocumentEditWindow(docId, _employeeId);
            if (editWindow.ShowDialog() == true)
            {
                LoadDocuments();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgDocuments.SelectedItem == null)
            {
                MessageBox.Show("Выберите документ для удаления");
                return;
            }

            if (MessageBox.Show("Удалить выбранный документ?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            DataRowView row = (DataRowView)dgDocuments.SelectedItem;
            int docId = Convert.ToInt32(row["Id"]);

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = $"DELETE FROM Documents WHERE Id = {docId}";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Документ удалён");
                    LoadDocuments();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadDocuments();
        }
    }
}