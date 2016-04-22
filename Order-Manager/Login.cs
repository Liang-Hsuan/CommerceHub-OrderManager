using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using Order_Manager.mainForms;

namespace Order_Manager
{
    /*
     * The main entry of the application -> login board
     */
    public partial class Login : Form
    {
        // boolean flag that determine if the program has to remember user input
        private bool remember = Properties.Settings.Default.Remember;

        public Login()
        {
            InitializeComponent();

            if (!remember) return;

            usernameTextbox.Text = Properties.Settings.Default.Username;
            passwordTextbox.Text = Properties.Settings.Default.Password;
            rememberCheckbox.Checked = true;
        }

        /* sign in button event that check if the user put in right credentials */
        private void signInButton_Click(object sender, EventArgs e)
        {
            // set invalid inidicator to invisible
            invalidLabel.Visible = false;

            // get username and password
            string userName = usernameTextbox.Text.Trim();
            string password = passwordTextbox.Text;

            // error check
            if (userName == "" || password == "")
            {
                MessageBox.Show("Please enter Username and Password please", "Sorry", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // initialize connection string
            string[] connectionString =
            {
                "Data Source=kpjvpp867r.database.windows.net; Initial Catalog=OrderHub;Integrated Security=False;User ID=" + userName + ";Password=" + password + ";Connect Timeout=60;Encrypt=False;TrustServerCertificate=False;MultipleActiveResultSets=true;",
                "Data Source=kpjvpp867r.database.windows.net;Initial Catalog=Design_Database;Integrated Security=False;User ID=" + userName + ";Password=" + password + ";Connect Timeout=60;Encrypt=False;TrustServerCertificate=False;",
                "Data Source=kpjvpp867r.database.windows.net;Initial Catalog=ChannelPartner_Database;Integrated Security=False;User ID=" + userName + ";Password=" + password + ";Connect Timeout=60;Encrypt=False;TrustServerCertificate=False;"
            };

            SqlConnection[] connection = { new SqlConnection(connectionString[0]), new SqlConnection(connectionString[1]), new SqlConnection(connectionString[2]) };

            // if connections are true -> open sku manager
            if (IsConnected(connection[0]) && IsConnected(connection[1]) && IsConnected(connection[2]))
            {
                Credentials.OrderHubCon = connectionString[0];
                Credentials.DesignCon = connectionString[1];
                Credentials.AscmCon = connectionString[2];
                Properties.Settings.Default.Username = userName;
                Properties.Settings.Default.Password = password;
                Hide();
                new Main(this).Show();
            }
            else
                invalidLabel.Visible = true;

            Properties.Settings.Default.Remember = remember;
        }

        /* a supporting method that see if the connection is valid or not */
        private static bool IsConnected(IDbConnection connection)
        {
            try
            {
                connection.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /* remember check box event that determine if the program will remember user's credentials */
        private void rememberCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            remember = rememberCheckbox.Checked;
        }
    }
}
