using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Data;

namespace LawstreamUpdate.Classes
{
    /// <summary>
    /// Class to hold and manage the database and connection details. We will pass this object
    /// around to any other object that requires comms with the database.
    /// </summary>
    public class DBConnection
    {

        #region Members

        const string _registryFolder = "SOFTWARE\\Cube Consulting";
        const string _registryFolderSubKey = "LawstreamUpdate";

        // Constants for the encryption / decrpytion of the password for DB access
        const string _passPhrase = "Pas5pr@se";               // can be any string
        const string _saltValue = "s@1tValue";                // can be any string
        const string _hashAlgorithm = "SHA1";                 // can be "MD5"
        const int _passwordIterations = 2;                    // can be any number
        const string _initVector = "@1B2c3D4e5F6g7H8";        // must be 16 bytes
        const int _keySize = 256;                             // can be 192 or 128

        string _server = "";
        string _database = "";
        string _username = "";
        string _password = "";
        bool _windowsAuth = true;

        bool _showAtStartup = true;

        string _connStr = "";
        SqlConnection _conn = new SqlConnection();

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor, unlikely to be utilised as we can use the constructor below
        /// to set the connection details at the same time.
        /// </summary>
        public DBConnection()
        {
            try
            {
                // Try to load the connection parameters from the registry
                Load();

                if (IsValid)
                {
                    // Test the connection string
                    _conn.Open();
                    _conn.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }

        /// <summary>
        /// Default constructor, unlikely to be utilised as we can use the constructor below
        /// to set the connection details at the same time.
        /// </summary>
        /// <param name="testConn">if set to <c>true</c> [test conn].</param>
        public DBConnection(bool testConn)
        {
            try
            {
                // Try to load the connection parameters from the registry
                Load();

                if ((testConn) && (IsValid))
                {
                    // Test the connection string
                    _conn.Open();
                    _conn.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }

        /// <summary>
        /// Main constructor for this class. When used, it will try to also open and then close the
        /// connection to ensure that we have a valid connection string.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="database"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="windowsAuthentication"></param>
        public DBConnection(string server, string database, string username, string password, bool windowsAuthentication)
        {
            try
            {
                // Store the values for the connection string
                _server = server;
                _database = database;
                _username = username;
                _password = password;
                _windowsAuth = windowsAuthentication;

                BuildConnectionString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Connection object property - read only as it is set from the _connString property value
        /// </summary>
        public SqlConnection Connection
        {
            get { return _conn; }
        }

        /// <summary>
        /// Connection string property
        /// </summary>
        public string ConnectionStr
        {
            get { return _connStr; }
        }

        /// <summary>
        /// Database property
        /// </summary>
        public string Database
        {
            get { return _database; }
            set { _database = value; }
        }

        /// <summary>
        /// IsValid property
        /// </summary>
        public bool IsValid
        {
            get 
            { 
                bool valid = true;
                if ((_server == "") || (_database == "") || ((_username == "") && (_windowsAuth == false)))
                {
                    valid = false;
                }

                return valid; 
            }
        }

        /// <summary>
        /// Connection string property
        /// </summary>
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        /// <summary>
        /// Database server property
        /// </summary>
        public string Server
        {
            get { return _server; }
            set { _server = value; }
        }    

        /// <summary>
        /// Connection string property
        /// </summary>
        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        /// <summary>
        /// Windows authentication set property
        /// </summary>
        public bool WindowsAuthentication
        {
            get { return _windowsAuth; }
            set { _windowsAuth = value; }
        }

        /// <summary>
        /// Show At Startup property
        /// </summary>
        public bool ShowAtStartup
        {
            get { return _showAtStartup; }
            set { _showAtStartup = value; }
        }

	    #endregion

        #region Methods

        /// <summary>
        /// Build up the connection string based on the user parameters.
        /// </summary>
        public void BuildConnectionString()
        {
            if (this.IsValid)
            {
                _connStr = "Data Source=" + _server + ";Initial Catalog=" + _database + ";";

                if (_windowsAuth)
                {
                    _connStr += "Integrated Security=SSPI;";
                }
                else
                {
                    _connStr += "User Id=" + _username + ";Password=" + _password;
                }
                _conn = new SqlConnection(_connStr);

                // Test the connection string
                _conn.Open();
                _conn.Close();
            }
        }

        /// <summary>
        /// Attempts to close the database connection. We have left opening and closing as a "manual" process
        /// so that we leave the control up to the calling object.
        /// </summary>
        public void Close()
        {
            if ((_conn.State != System.Data.ConnectionState.Closed) && (_conn.State != System.Data.ConnectionState.Broken))
            {
                _conn.Close();
            }

        }

        /// <summary>
        /// Form load event. We read the values from the registry so that users aren't having to
        /// continually re-enter the connection info.
        /// </summary>
        public string Load()
        {
            string folder = string.Empty;

            RegistryKey hkcu = Registry.CurrentUser;
            hkcu = hkcu.OpenSubKey(_registryFolder + "\\" + _registryFolderSubKey, false);

            if (hkcu != null)
            {
                Object obj = hkcu.GetValue("Server", "");
                _server = obj.ToString();

                obj = hkcu.GetValue("Username", "");
                _username = obj.ToString();

                obj = hkcu.GetValue("Password", "");
                string password = obj.ToString();
                if (password != "")
                {
                    _password = Encryption.Decrypt(password, _passPhrase, _saltValue,
                                                        _hashAlgorithm, _passwordIterations,
                                                        _initVector, _keySize);
                }
                
                obj = hkcu.GetValue("Database", "");
                _database = obj.ToString();

                obj = hkcu.GetValue("WindowsAuthentication", true);
                _windowsAuth = Convert.ToBoolean(obj);

                _connStr = "Data Source=" + _server + ";Initial Catalog=" + _database + ";";
                
                if (_windowsAuth)
                {
                    _connStr += "Integrated Security=SSPI;";
                }
                else
                {
                    _connStr += "User Id=" + _username + ";Password=" + _password;
                }

                _conn = new SqlConnection(_connStr);

                obj = hkcu.GetValue("Folder", "");
                folder = obj.ToString();
            }

            return folder;
        }


        /// <summary>
        /// Attempts to open the database connection. We have left opening and closing as a "manual" process
        /// so that we leave the control up to the calling object.
        /// </summary>
        public void Open()
        {
            if ((_server == "") || (_database == "") || ((_username == "") && (_windowsAuth == false)))
            {
                throw new Exception("Database access parameters are not defined!");
            }

            if (_conn == null)
            {
                throw new Exception("Database connection is not initialised.");
            }

            BuildConnectionString();
            _conn.Open();

        }

        /// <summary>
        /// clears the database connection parameters in the registry.
        /// </summary>
        public void Clear()
        {
            RegistryKey hkcu = Registry.CurrentUser;
            RegistryKey regKey = hkcu.OpenSubKey(_registryFolder, true);

            if (regKey != null)
            {
                regKey.DeleteSubKeyTree(_registryFolderSubKey);
            }
        }

        /// <summary>
        /// Save the database connection parameters to the registry for use later on.
        /// </summary>
        public void Save(string folder)
        {
            RegistryKey hkcu = Registry.CurrentUser;
            RegistryKey regKey = hkcu.OpenSubKey(_registryFolder + "\\" + _registryFolderSubKey, true);

            if (regKey == null)
            {
                hkcu.CreateSubKey(_registryFolder + "\\" + _registryFolderSubKey); // Create the key
                regKey = hkcu.OpenSubKey(_registryFolder + "\\" + _registryFolderSubKey, true);
            }

            // Save each of the values as required.
            regKey.SetValue("Server", _server);
            regKey.SetValue("Database", _database);

            if (_username != "")
            {
                regKey.SetValue("Username", _username);
            }

            if (_password != "")
            {
                regKey.SetValue("Password", Encryption.Encrypt(_password, _passPhrase,
                                                    _saltValue, _hashAlgorithm,
                                                    _passwordIterations, _initVector,
                                                    _keySize));
            }

            regKey.SetValue("WindowsAuthentication", _windowsAuth);

            regKey.SetValue("Folder", folder);
        }

        /// <summary>
        /// Executes an SQL statement against the database. The function will either run a "normal"
        /// execute function or a scalar execute function as requested. The scalar execute function
        /// is useful when we need to find out the identity field for the inserted record.
        /// </summary>
        /// <param name="SQL">The SQL statement to execute</param>
        /// <param name="conn">The connection object to use, passed in via the interface object</param>
        /// <param name="executeScalar">Boolean flag indicating whether to execute and return the
        /// identity field</param>
        /// <returns></returns>
        public int ExecuteSQL(string SQL, bool executeScalar)
        {
            int retValue = 0;

            if (this.IsValid)
            {
                // SQL Server data client
                SqlCommand cmdSQL = new SqlCommand(SQL, this.Connection);
                cmdSQL.CommandTimeout = 0;

                if (executeScalar)
                {
                    try
                    {
                        retValue = (int)cmdSQL.ExecuteScalar();
                    }
                    catch
                    {
                        retValue = 0;
                    }
                }
                else
                {
                    retValue = cmdSQL.ExecuteNonQuery();
                }
            }

            return retValue;
        }

        /// <summary>
        /// Executes an SQL Select statement against the database. The command is executed against
        /// the database and the results are returned in a DataReader. As with all of the functions
        /// in this class, we use an interface object (IDataReader in this case) because we are
        /// using different types of connection objects, depending on the database.
        /// A DataReader is used for efficiency purposes (as opposed to the memory-hungry DataSet).
        /// </summary>
        /// <param name="SQL">The SQL Select statement to execute</param>
        /// <returns>An SqlDataReader object streaming the selected data</returns>
        public SqlDataReader SelectData(string SQL)
        {
            if (this.IsValid)
            {
                SqlDataReader dr;

                // SQL Server data client
                SqlCommand cmdSQL = new SqlCommand(SQL, this.Connection);
                dr = cmdSQL.ExecuteReader();

                return dr;
            }
            else
            {
                throw new Exception("Connection parameters are invalid!");
            }
        }

        #endregion
    }
}
