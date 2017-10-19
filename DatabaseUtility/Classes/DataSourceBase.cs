using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Xml.Serialization;
using System.IO;

namespace LawstreamUpdate.Classes
{
    /// <summary>
    /// This class represents a database system. This system may be either a destination DB
    /// or any external data source for importing data into the destination database.
    /// </summary>
    [Serializable, DefaultPropertyAttribute("DatabaseType"), XmlInclude(typeof(DataSourceExcel)), XmlInclude(typeof(DataSourceTextFile))]
    public class DataSourceBase : IDataSource
    {
        #region Members

        /// <summary>
        /// Flag to determine whether the object properties have been modified
        /// </summary>
        protected bool _isDirty;

        /// <summary>
        /// The name of the system
        /// </summary>
        protected string _name;

        /// <summary>
        /// Connection type
        /// </summary>
        protected ConstantValues.ConnectionType _connType;

        /// <summary>
        /// The item type that we're browsing (tables / views)
        /// </summary>
        protected ConstantValues.SourceType _dataSourceType;

        /// <summary>
        /// Flag to determine the validity of the connection parameters (ie are they there and complete?)
        /// </summary>
        protected bool _validConnection = false;

        /// <summary>
        /// The character to use for bracketing in the SQL statement - Oracle requires "[]", SQL Server uses quotes
        /// </summary>
        protected ConstantValues.BracketType _bracketChar;

        /// <summary>
        /// The char to use when appending data for concatenated fields - Oracle uses "||", SQL Server uses "+"
        /// </summary>
        protected ConstantValues.SQLAppendCharType _appendChar;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor - required for the XML deserialisation
        /// </summary>
        public DataSourceBase()
        {
            _isDirty = false;
            _name = "";
            
            _validConnection = false;
            _bracketChar = ConstantValues.BracketType.SquareBrackets;
            _appendChar = ConstantValues.SQLAppendCharType.Plus;
            _dataSourceType = ConstantValues.SourceType.Tables;
        }

        /// <summary>
        /// Additional constructor
        /// </summary>
        /// <param name="name">Name of the system that we will be working with. Must be unique.</param>
        public DataSourceBase(string name)
        {
            _isDirty = false;

            _name = name;
            
            _validConnection = false;
            _bracketChar = ConstantValues.BracketType.SquareBrackets;
            _appendChar = ConstantValues.SQLAppendCharType.Plus;
            _dataSourceType = ConstantValues.SourceType.Tables;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Flag to get or set the IsDirty state
        /// </summary>
        [XmlIgnore(), BrowsableAttribute(false)]
        public bool IsDirty
        {
            get
            {
                return _isDirty;
            }
            set
            {
                _isDirty = value;
            }
        }

        /// <summary>
        /// Property to construct the connection string. The string returned will be catered to the
        /// connection type (SQL Server, Oracle OLE DB or Access OLE DB).
        /// </summary>
        [XmlIgnore(),BrowsableAttribute(false)]
        protected virtual string ConnectionString
        {
            get
            {
                return "";
            }
        }

        /// <summary>
        /// The unique name of the database system
        /// </summary>
        [XmlAttribute("BracketChar"), DisplayName("Bracketing"), Browsable(false)]
        public ConstantValues.BracketType BracketChar
        {
            get
            {
                return _bracketChar;
            }
            set
            {
                _bracketChar = value;
                _isDirty = true;
            }
        }

        /// <summary>
        /// The character used for appending strings
        /// </summary>
        [XmlAttribute("SQLAppendchar"), DisplayName("Append character"),  Browsable(false)]
        public ConstantValues.SQLAppendCharType SQLAppendchar
        {
            get
            {
                return _appendChar;
            }
            set
            {
                _appendChar = value;
                _isDirty = true;
            }
        }

        /// <summary>
        /// The unique name of the database system
        /// </summary>
        [XmlAttribute("Name"), Category("All"), DescriptionAttribute("Name of this data source - must be unique")]
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                _validConnection = false;
                _isDirty = true;
            }
        }

        /// <summary>
        /// Stores the connection type for this system. Choices are SQL Server, Oracle / MS Access (both OLEDB)
        /// or text files (CSV, Tab or semi-colon delimited).
        /// </summary>
        [XmlIgnore(), BrowsableAttribute(false)]
        internal ConstantValues.ConnectionType DatabaseType
        {
            get
            {
                return _connType;
            }
            set
            {
                _connType = value;
                if (_connType == ConstantValues.ConnectionType.Oracle)
                {
                    _bracketChar = ConstantValues.BracketType.Quotes;
                    _appendChar = ConstantValues.SQLAppendCharType.DoubleBars;
                }
                else
                {
                    _bracketChar = ConstantValues.BracketType.SquareBrackets;
                    _appendChar = ConstantValues.SQLAppendCharType.Plus;
                }
                _validConnection = false;
                _isDirty = true;
            }
        }

        /// <summary>
        /// Stores the connection type for this system. Choices are SQL Server, Oracle / MS Access (both OLEDB)
        /// or text files (CSV, Tab or semi-colon delimited).
        /// </summary>
        [XmlAttribute("ConnectionType"), DisplayName("Connection type"), Category("All"), DescriptionAttribute("The type of connection to use for this external system")]
        public ConstantValues.ConnectionType Datasource
        {
            get
            {
                return _connType;
            }
        }

        /// <summary>
        /// Global definiton for the source type. Defines whether we use Tables or Views.
        /// </summary>
        [XmlIgnore(), Browsable(false)]
        internal ConstantValues.SourceType DataSourceTypeInternal
        {
            get
            {
                return _dataSourceType;
            }
            set
            {
                _dataSourceType = value;
                _isDirty = true;
            }
        }

        /// <summary>
        /// Property to determine whether or not the object has enough information to successfully
        /// try to connect to the server / database.
        /// </summary>
        [XmlIgnore(), BrowsableAttribute(false)]
        public virtual bool IsValid
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Opens the connection to the database based on the parameters that have been set 
        /// up by the user.
        /// </summary>
        /// <returns>IDBConnection object - use an interface object as we have different
        /// types of connection base objects, depending on the database type</returns>
        public IDbConnection NewConnection()
        {
            if (this.IsValid)
            {
                // SQL Server - use the SQL Client connection object.
                if (this.DatabaseType == ConstantValues.ConnectionType.SQL_Server)
                {
                    SqlConnection.ClearAllPools();
                    SqlConnection connSQL = new SqlConnection(this.ConnectionString);
                    connSQL.Open();

                    return connSQL;
                }

                // OLEDB clients
                else if ((this.DatabaseType == ConstantValues.ConnectionType.Oracle)
                            || (this.DatabaseType == ConstantValues.ConnectionType.MS_Access)
                            || (this.DatabaseType == ConstantValues.ConnectionType.Text_File)
                            || (this.DatabaseType == ConstantValues.ConnectionType.MS_Excel))
                {
                    OleDbConnection.ReleaseObjectPool();
                    OleDbConnection connOLEDB = new OleDbConnection(this.ConnectionString);
                    connOLEDB.Open();
                    
                    return connOLEDB;
                }
                else
                {
                    throw new Exception("Invalid connection type encountered!");
                }

            }
            else
            {
                throw new Exception("Connection parameters are invalid!");
            }
        }
                
        /// <summary>
        /// Function to return the column schema information for a given table. This will be used by
        /// this object as well as any inheriting objects. Note that the GetSchema function
        /// returns different types of information depending upon the database it is connected
        /// to so we try to stick to utilising only generic information. One notable item missing
        /// is any indication of the Primary Key - IMO a major overthought on the part of .NET.
        /// </summary>
        /// <param name="tableAndSchema">The table and schema.</param>
        /// <param name="colName">Name of the col.</param>
        /// <returns>
        /// A DataTable object containing the column schema defintions.
        /// </returns>
        /// <exception cref="System.Exception">Cannot read schema information for this table!</exception>
        public DataTable ColumnSchema(string tableAndSchema, string colName = null)
        {
            DataTable cols = new DataTable();
            IDbConnection conn = this.NewConnection();
            string SQL = string.Empty;

            if ((this.DatabaseType == ConstantValues.ConnectionType.Text_File) || (this.DatabaseType == ConstantValues.ConnectionType.MS_Excel)
                || (this.DatabaseType == ConstantValues.ConnectionType.MS_Access))
            {
                // No schemas for Excel / Text files!!
                string tableName = tableAndSchema;
                string[] tableSchemaSplitter = tableAndSchema.Split('.');

                if (tableSchemaSplitter.GetUpperBound(0) > 0)
                {
                    tableName = tableSchemaSplitter[1];
                }

                // Pick up the schema for this table from the destination database.
                string[] restrictions = new string[4] { null, null, tableName, colName };

                SQL = "SELECT * FROM [" + tableName + "]";
                if (!string.IsNullOrWhiteSpace(colName))
                {
                    SQL = "SELECT " + colName + " FROM [" + tableName + "]";
                }
            }
            else
            {
                string tableSchema = "dbo";         // Default
                string tableName = tableAndSchema;
                string[] tableSchemaSplitter = tableAndSchema.Split('.');

                if (tableSchemaSplitter.GetUpperBound(0) > 0)
                {
                    tableSchema = tableSchemaSplitter[0];
                    tableName = tableSchemaSplitter[1];

                    // Remove the "[" "]" brackets, they just mess us up later on!!
                    tableSchema = tableSchema.Replace("[", string.Empty);
                    tableSchema = tableSchema.Replace("]", string.Empty);

                    tableName = tableName.Replace("[", string.Empty);
                    tableName = tableName.Replace("]", string.Empty);
                }

                // Pick up the schema for this table from the destination database.
                string[] restrictions = new string[4] { null, tableSchema, tableName, colName };
                
                SQL = "SELECT * FROM [" + tableSchema + "].[" + tableName + "]";
                if (!string.IsNullOrWhiteSpace(colName))
                {
                    SQL = "SELECT [" + colName + "] FROM [" + tableSchema + "].[" + tableName + "]";
                }
            }

            try
            {
                // SQL Server - use the SQL Client objects.
                if (this.DatabaseType == ConstantValues.ConnectionType.SQL_Server)
                {
                    SqlConnection tempConn = (SqlConnection)conn;
                    
                    SqlCommand sqlCmd = new SqlCommand();
                    sqlCmd.CommandText = SQL;
                    sqlCmd.Connection = tempConn;
                    SqlDataReader sqlReader = sqlCmd.ExecuteReader(CommandBehavior.SchemaOnly);

                    //Retrieve column schema into a DataTable.
                    cols = sqlReader.GetSchemaTable();

                    sqlReader.Close();
                }

                // Use the OLEDB objects.
                else if ((this.DatabaseType == ConstantValues.ConnectionType.Oracle)
                            || (this.DatabaseType == ConstantValues.ConnectionType.MS_Access)
                            || (this.DatabaseType == ConstantValues.ConnectionType.Text_File)
                            || (this.DatabaseType == ConstantValues.ConnectionType.MS_Excel))
                {
                    OleDbConnection tempConn = (OleDbConnection)conn;
                    
                    OleDbCommand oleDBCmd = new OleDbCommand();
                    oleDBCmd.CommandText = SQL;
                    oleDBCmd.Connection = tempConn;
                    OleDbDataReader oleDBReader = oleDBCmd.ExecuteReader(CommandBehavior.SchemaOnly); 

                    //Retrieve column schema into a DataTable.
                    cols = oleDBReader.GetSchemaTable();

                    oleDBReader.Close();
                }

                if (cols == null)
                {
                    throw new Exception("Cannot read schema information for this table!");
                }

                return cols;
            }
            finally
            {
                conn.Close();
            }

        }

        /// <summary>
        /// Retrieve the table schema from the database. This function also further splits
        /// itself into retrieving either the table information or view information, depending
        /// on the database source that has been defined for this system.
        /// </summary>
        /// <param name="allSchemas">if set to <c>true</c> [display all schemas in the database].</param>
        /// <param name="tblName">Name of the table.</param>
        /// <returns>
        /// A DataTable object containing the table schema defintions.
        /// </returns>
        /// <exception cref="Exception">Cannot read schema information, database permissions may be insufficient.</exception>
        /// <exception cref="System.Exception">Cannot read schema information, database permissions may be insufficient.</exception>
        public DataTable TableSchema(bool allSchemas, string tblName = "")
        {
            DataTable tables = new DataTable();
            IDbConnection conn = this.NewConnection();

            try
            {
                // SQL Server - use the SQL Client objects.
                if (this.DatabaseType == ConstantValues.ConnectionType.SQL_Server)
                {
                    SqlConnection tempConn = (SqlConnection)conn;
                    if (this.DataSourceTypeInternal == ConstantValues.SourceType.Tables)
                    {
                        // Return tables information
                        if (allSchemas)
                        {
                            if (string.IsNullOrWhiteSpace(tblName))
                            {
                                tables = tempConn.GetSchema(SqlClientMetaDataCollectionNames.Tables, new string[] { null, null, null, "BASE TABLE" });
                            }
                            else
                            {
                                tables = tempConn.GetSchema(SqlClientMetaDataCollectionNames.Tables, new string[] { null, null, tblName, "BASE TABLE" });
                            }
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(tblName))
                            {
                                tables = tempConn.GetSchema(SqlClientMetaDataCollectionNames.Tables, new string[] { null, "dbo", null, "BASE TABLE" });
                            }
                            else
                            {
                                tables = tempConn.GetSchema(SqlClientMetaDataCollectionNames.Tables, new string[] { null, "dbo", tblName, "BASE TABLE" });
                            }
                        }
                    }
                    else
                    {
                        // Return views information
                        if (allSchemas)
                        {
                            if (string.IsNullOrWhiteSpace(tblName))
                            {
                                tables = tempConn.GetSchema(SqlClientMetaDataCollectionNames.Views, new string[] { null, null, null });
                            }
                            else
                            {
                                tables = tempConn.GetSchema(SqlClientMetaDataCollectionNames.Views, new string[] { null, null, tblName });
                            }
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(tblName))
                            {
                                tables = tempConn.GetSchema(SqlClientMetaDataCollectionNames.Views, new string[] { null, "dbo", null });
                            }
                            else
                            {
                                tables = tempConn.GetSchema(SqlClientMetaDataCollectionNames.Views, new string[] { null, "dbo", tblName });
                            }
                        }
                    }
                }

                // Use the OLEDB objects.
                else if ((this.DatabaseType == ConstantValues.ConnectionType.Oracle)
                            || (this.DatabaseType == ConstantValues.ConnectionType.MS_Access)
                            || (this.DatabaseType == ConstantValues.ConnectionType.Text_File)
                            || (this.DatabaseType == ConstantValues.ConnectionType.MS_Excel))
                {
                    OleDbConnection tempConn = (OleDbConnection)conn;
                    if (this.DataSourceTypeInternal == ConstantValues.SourceType.Tables)
                    {
                        // Return tables information
                        if (string.IsNullOrWhiteSpace(tblName))
                        {
                            tables = tempConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new string[] { null, null, null, "TABLE" });
                        }
                        else
                        {
                            tables = tempConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new string[] { null, null, tblName, "TABLE" });
                        }
                    }
                    else if (this.DataSourceTypeInternal == ConstantValues.SourceType.Views)
                    {
                        // Return views information
                        if (this.DatabaseType == ConstantValues.ConnectionType.Oracle)
                        {
                            if (string.IsNullOrWhiteSpace(tblName))
                            {
                                tables = tempConn.GetOleDbSchemaTable(OleDbSchemaGuid.Views, new string[] { null, null, null, null });
                            }
                            else
                            {
                                tables = tempConn.GetOleDbSchemaTable(OleDbSchemaGuid.Views, new string[] { null, null, tblName, null });
                            }
                        }
                        else if (this.DatabaseType == ConstantValues.ConnectionType.MS_Access)
                        {
                            if (string.IsNullOrWhiteSpace(tblName))
                            {
                                tables = tempConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new string[] { null, null, null, "VIEW" });
                            }
                            else
                            {
                                tables = tempConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new string[] { null, null, tblName, "VIEW" });
                            }
                        }
                        else
                        {
                            // There are no views available through Access / Excel / Text files so we just
                            // default back to Tables.
                            if (string.IsNullOrWhiteSpace(tblName))
                            {
                                tables = tempConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new string[] { null, null, null, "TABLE" });
                            }
                            else
                            {
                                tables = tempConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new string[] { null, null, tblName, "TABLE" });
                            }
                        }
                    }
                    // Synonyms EXIST in Oracle but they're painful. So we'll want to pretend they don't exist! ;)
                    //else if (this.DataSourceTypeInternal == ConstantValues.SourceType.Synonyms)
                    //{
                    //    tables = tempConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new string[] { null, null, null, "SYNONYM" });
                    //}
                }

                if (tables == null)
                {
                    throw new Exception("Cannot read schema information, database permissions may be insufficient.");
                }

                return tables;
            }
            finally
            {
                conn.Close();
            }
        }

        /// <summary>
        /// Retrieve the views schema from the database. This function ONLY specifically gets view information.
        /// Used when having to hook custom SQL to a table field that HAS NO FK INFO IN THE DATABASE (ie a manually-added link!!)
        /// </summary>
        /// <param name="allSchemas">if set to <c>true</c> [display all schemas in the database].</param>
        /// <returns>
        /// A DataTable object containing the table schema defintions.
        /// </returns>
        /// <exception cref="System.Exception">Cannot read schema information, database permissions may be insufficient.</exception>
        public DataTable ViewSchema(bool allSchemas)
        {
            DataTable tables = new DataTable();
            IDbConnection conn = this.NewConnection();

            try
            {
                // SQL Server - use the SQL Client objects.
                if (this.DatabaseType == ConstantValues.ConnectionType.SQL_Server)
                {
                    SqlConnection tempConn = (SqlConnection)conn;
                    
                    // Return views information
                    tables = tempConn.GetSchema(SqlClientMetaDataCollectionNames.Views);
                }

                // Use the OLEDB objects.
                else if ((this.DatabaseType == ConstantValues.ConnectionType.Oracle)
                            || (this.DatabaseType == ConstantValues.ConnectionType.MS_Access)
                            || (this.DatabaseType == ConstantValues.ConnectionType.Text_File)
                            || (this.DatabaseType == ConstantValues.ConnectionType.MS_Excel))
                {
                    OleDbConnection tempConn = (OleDbConnection)conn;
                    // Return views information
                    if (this.DatabaseType == ConstantValues.ConnectionType.Oracle)
                    {
                        tables = tempConn.GetOleDbSchemaTable(OleDbSchemaGuid.Views, new string[] { null, null, null, null });
                    }
                    else if (this.DatabaseType == ConstantValues.ConnectionType.MS_Access)
                    {
                        tables = tempConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new string[] { null, null, null, "VIEW" });
                    }
                    else
                    {
                        // There are no views available through Access / Excel / Text files so we just
                        // default back to Tables.
                        throw new Exception("Views are invalid for Excel / Text files!");
                    }
                    
                    // Synonyms EXIST in Oracle but they're painful. So we'll want to pretend they don't exist! ;)
                    //else if (this.DataSourceTypeInternal == ConstantValues.SourceType.Synonyms)
                    //{
                    //    tables = tempConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new string[] { null, null, null, "SYNONYM" });
                    //}
                }

                if (tables == null)
                {
                    throw new Exception("Cannot read schema information, database permissions may be insufficient.");
                }

                return tables;
            }
            finally
            {
                conn.Close();
            }
        }
        
        /// <summary>
        /// Executes an SQL Select statement against the database. The command is executed against
        /// the database and the results are returned in a DataReader. As with all of the functions
        /// in this class, we use an interface object (IDataReader in this case) because we are
        /// using different types of connection objects, depending on the database.
        /// A DataReader is used for efficiency purposes (as opposed to the memory-hungry DataSet).
        /// </summary>
        /// <param name="SQL">The SQL Select statement to execute</param>
        /// <param name="conn">The connection object to the data to use. Note that a conection
        /// can only have a single DataReader on it.</param>
        /// <returns>An IDataReader object streaming the selected data</returns>
        public IDataReader SelectData(string SQL, IDbConnection conn)
        {
            if (this.IsValid)
            {
                IDataReader dr = null;

                // SQL Server data client
                if (this.DatabaseType == ConstantValues.ConnectionType.SQL_Server)
                {
                    SqlCommand cmdSQL = new SqlCommand(SQL, (SqlConnection)conn);
                    dr = cmdSQL.ExecuteReader();
                }

                // OLEDB data client
                else if ((this.DatabaseType == ConstantValues.ConnectionType.Oracle)
                        || (this.DatabaseType == ConstantValues.ConnectionType.MS_Access)
                        || (this.DatabaseType == ConstantValues.ConnectionType.Text_File)
                        || (this.DatabaseType == ConstantValues.ConnectionType.MS_Excel))
                {
                    try
                    {
                        OleDbCommand cmdOLEDB = new OleDbCommand(SQL, (OleDbConnection)conn);
                        dr = cmdOLEDB.ExecuteReader();
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.ToLower() == "unknown")
                        {
                            throw new Exception("Unknown error - it appears that a field is too small for the data it contains:" + Environment.NewLine + Environment.NewLine + SQL);
                        }
                        else
                        {
                            throw new Exception(ex.Message);
                        }
                    }
                }
                else
                {
                    throw new Exception("Invalid connection type encountered!");
                }

                return dr;

            }
            else
            {
                throw new Exception("Connection parameters are invalid!");
            }
        }

        /// <summary>
        /// Override implementation for the ExecuteSQL function that executes the
        /// function without a Transaction object.
        /// </summary>
        /// <param name="SQL">The SQL statement to execute</param>
        /// <param name="executeScalar">Boolean flag indicating whether to execute the SQL as a scalar function</param>
        /// <returns></returns>
        public int ExecuteSQL(string SQL, bool executeScalar)
        {
            using (IDbConnection conn = this.NewConnection())
            {
                return ExecuteSQL(SQL, conn, null, executeScalar);
            }
        }

        /// <summary>
        /// Executes an SQL statement against the database. The function will either run a "normal"
        /// execute function or a scalar execute function as requested. The scalar execute function
        /// is useful when we need to find out the identity field for the inserted record.
        /// </summary>
        /// <param name="SQL">The SQL statement to execute</param>
        /// <param name="trans">A transaction object to attach to the command as required</param>
        /// <param name="executeScalar">Boolean flag indicating whether to execute as a scalar function</param>
        /// <returns></returns>
        public int ExecuteSQL(string SQL, IDbTransaction trans, bool executeScalar)
        {
            using (IDbConnection conn = this.NewConnection())
            {
                return ExecuteSQL(SQL, conn, trans, executeScalar);
            }
        }

        /// <summary>
        /// Executes an SQL statement against the database. The function will either run a "normal"
        /// execute function or a scalar execute function as requested. The scalar execute function
        /// is useful when we need to find out the identity field for the inserted record.
        /// </summary>
        /// <param name="SQL">The SQL statement to execute</param>
        /// <param name="conn">The connection object to use, passed in via the interface object</param>
        /// <param name="trans">A transaction object to attach to the command as required</param>
        /// <param name="executeScalar">Boolean flag indicating whether to execute the function as a scalar function</param>
        /// <returns></returns>
        public int ExecuteSQL(string SQL, IDbConnection conn, IDbTransaction trans, bool executeScalar)
        {
            int retValue = 0;
       
            if (this.IsValid)
            {
                // SQL Server data client
                if (this.DatabaseType == ConstantValues.ConnectionType.SQL_Server)
                {
                    SqlCommand cmdSQL = new SqlCommand(SQL, (SqlConnection)conn);
                    cmdSQL.CommandTimeout = 0;

                    // Wrap the execute statement in a transaction is applicable
                    if (trans != null)
                    {
                        cmdSQL.Transaction = (SqlTransaction)trans;
                    }

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

                // OLEDB data client
                else if ((this.DatabaseType == ConstantValues.ConnectionType.Oracle)
                        || (this.DatabaseType == ConstantValues.ConnectionType.MS_Access)
                        || (this.DatabaseType == ConstantValues.ConnectionType.Text_File)
                        || (this.DatabaseType == ConstantValues.ConnectionType.MS_Excel))
                {
                    OleDbCommand cmdOLEDB = new OleDbCommand(SQL, (OleDbConnection)conn);
                    cmdOLEDB.CommandTimeout = 0;

                    // Wrap the execute statement in a transaction is applicable
                    if (trans != null)
                    {
                        cmdOLEDB.Transaction = (OleDbTransaction)trans;
                    }

                    if (executeScalar)
                    {
                        try
                        {
                            retValue = (int)cmdOLEDB.ExecuteScalar();
                        }
                        catch
                        {
                            retValue = 0;
                        }
                    }
                    else
                    {
                        retValue = cmdOLEDB.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                throw new Exception("Invalid connection type encountered!");
            }

            return retValue;
        }


        /// <summary>
        /// Function to test the connection parameters to ensure that we have set up a valid 
        /// connection to the system.
        /// </summary>
        /// <returns>True if the connection attempt succeeded, False if it did not.</returns>
        public bool TestConnection(out string error)
        {
            try
            {
                error = "";

                if (this.IsValid)
                {
                    IDbConnection conn = this.NewConnection();
                    conn.Close();

                    _validConnection = true;
                    return true;
                }
                else
                {
                    error = "Invalid / missing connection parameters";
                    return false;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        #endregion
    }
}
