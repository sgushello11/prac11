using System;
using System.Data.SqlClient;
using System.Windows;

namespace DocumentFlowApp
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                tbError.Text = "Введите логин и пароль";
                return;
            }

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = $"SELECT * FROM Users WHERE Login = '{login}' AND PasswordHash = '{password}'";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string role = reader["Role"].ToString();
                        int userId = Convert.ToInt32(reader["Id"]);
                        int employeeId = Convert.ToInt32(reader["EmployeeId"]);
                        reader.Close();

                        MainWindow mainWindow = new MainWindow(role, userId, employeeId, login);
                        mainWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        tbError.Text = "Неверный логин или пароль";
                    }
                }
            }
            catch (Exception ex)
            {
                tbError.Text = "Ошибка: " + ex.Message;
            }
        }
    }
}