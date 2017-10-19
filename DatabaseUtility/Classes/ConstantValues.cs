using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace LawstreamUpdate.Classes
{
    /// <summary>
    /// Static helper class that contains the constants and other static values that will be
    /// used throughout the system. We have kept them in a single class so as to hardcode as
    /// little as possible and to ensure that we maintain good cohesion with minimal coupling.
    /// </summary>
    public class ConstantValues
    {
        #region EnumeratedTypes

        /// <summary>
        /// Enum for defining the type of popup for the small edit box
        /// </summary>
        public enum EditType
        {
            /// <summary>
            /// Default value editing
            /// </summary>
            DefaultValue = 0,

            /// <summary>
            /// Editing to add a prefix
            /// </summary>
            Prefix = 1,

            /// <summary>
            /// Editing the column we're getting from the foreign key table
            /// </summary>
            ForeignTableColumnLookup = 2,

            /// <summary>
            /// Editing the FK TABLE we're needing (for when FK's aren't defined, allows us to do it manually)
            /// </summary>
            ForeignTableLookup = 3,

            /// <summary>
            /// If we are using a view or the FK table has no PK (with custom SQL etc), need to be able to manually set the PK
            /// </summary>
            ForeignTableColumnLookupSetKey = 4
        }

        /// <summary>
        /// The type of drag / drop operation we're undertaking - a column or a table.
        /// </summary>
        public enum DragType
        {
            /// <summary>
            /// Drag a column
            /// </summary>
            Column = 0,

            /// <summary>
            /// Drag the whole table
            /// </summary>
            Table = 1
        }
        
        /// <summary>
        /// Connection type of the database. Includes Excel, 
        /// text (flat) files etc.
        /// Note that Oracle and MS Access both utilise OLEDB data clients.
        /// </summary>
        public enum ConnectionType
        {
            /// <summary>
            /// SQL Server db
            /// </summary>
            SQL_Server = 0,

            /// <summary>
            /// Oracle db
            /// </summary>
            Oracle = 1,

            /// <summary>
            /// MS Access db
            /// </summary>
            MS_Access = 2,

            /// <summary>
            /// MS Excel
            /// </summary>
            MS_Excel = 3,

            /// <summary>
            /// Text files
            /// </summary>
            Text_File = 4
        }

        /// <summary>
        /// Enumerated type to determine the type of data to use - tables or views - in the
        /// database.
        /// </summary>
        public enum SourceType
        {
            /// <summary>
            /// Tables
            /// </summary>
            Tables = 0,

            /// <summary>
            /// Views
            /// </summary>
            Views = 1
        }

        /// <summary>
        /// Enumerated type to determine the version of the file we are viewing. Excel files are
        /// 2003 or 2007 and text files are determined by their delimiter type
        /// </summary>
        public enum ExcelVersionType
        {
            /// <summary>
            /// Excel 2003
            /// </summary>
            Excel2003 = 0,

            /// <summary>
            /// Excel 2007
            /// </summary>
            Excel2007 = 1
        }

        /// <summary>
        /// Enumerated type to determine the version of the text file we are viewing. Text files are
        /// determined by their delimiter type
        /// </summary>
        public enum VersionType
        {
            /// <summary>
            /// Commas
            /// </summary>
            CommaDelim = 0,

            /// <summary>
            /// Tabs
            /// </summary>
            TabDelim = 1,

            /// <summary>
            /// Semi colons
            /// </summary>
            SemiColonDelim = 2,

            /// <summary>
            /// Pipe (|)
            /// </summary>
            PipeDelim = 3
        }

        /// <summary>
        /// Enumerated type to determine the bracketing character to use.
        /// </summary>
        public enum BracketType
        {
            /// <summary>
            /// Oracle / SQL Server / Excel / MS Access
            /// </summary>
            SquareBrackets = 0,

            /// <summary>
            /// 
            /// </summary>
            Quotes = 1,

            /// <summary>
            /// Text files
            /// </summary>
            None = 2
        }

        /// <summary>
        /// Enumerated type to determine the bracketing character to use.
        /// </summary>
        public enum SQLAppendCharType
        {
            /// <summary>
            /// SQL Server, Access etc
            /// </summary>
            Plus = 0,

            /// <summary>
            /// Oracle
            /// </summary>
            DoubleBars = 1
        }

        /// <summary>
        /// Represents the application type we're uploading data for
        /// </summary>
        public enum ApplicationType
        {
            /// <summary>
            /// InControl
            /// </summary>
            InControl = 0,

            /// <summary>
            /// InFlight
            /// </summary>
            InFlight = 1,

            /// <summary>
            /// InTellimetrics
            /// </summary>
            InTellimetrics = 2,

            /// <summary>
            /// InTime
            /// </summary>
            InTime = 3,

            /// <summary>
            /// InTuition
            /// </summary>
            InTuition = 4,

            /// <summary>
            /// Unknown / multiple applications
            /// </summary>
            Unknown = 5,

            /// <summary>
            /// People only
            /// </summary>
            People = 6,

            /// <summary>
            /// Roles only
            /// </summary>
            Roles = 7,

            /// <summary>
            /// Rosters only
            /// </summary>
            Rosters = 8,

            /// <summary>
            /// Competencies only
            /// </summary>
            Competencies = 9,

            /// <summary>
            /// Compliances only
            /// </summary>
            Compliances = 10,

            /// <summary>
            /// Procedures only
            /// </summary>
            Procedures = 11,

            /// <summary>
            /// Flights only
            /// </summary>
            Flights = 12,

            /// <summary>
            /// Events only
            /// </summary>
            Events = 13,

            /// <summary>
            /// InTouch tables only
            /// </summary>
            InTouch = 14,

            /// <summary>
            /// Advanced tab only
            /// </summary>
            Advanced = 15
        }

        /// <summary>
        /// The size of the edit window to use - small is font 8ish and large is 10ish
        /// </summary>
        public enum EditSizeType
        {
            /// <summary>
            /// 
            /// </summary>
            Small = 0,

            /// <summary>
            /// 
            /// </summary>
            Large = 1
        }


        #endregion

        #region Members

        // General window size and position registry constants
        static string _registryFolder = "SOFTWARE\\INX Software\\Mapster ITC";
        static string _windowLeft = "Left";
        static string _windowTop = "Top";
        static string _windowWidth = "Width";
        static string _windowHeight = "Height";
        static string _windowState = "WindowState";
        static string _splitterDistance = "SplitterDistance";
        static string _splitterDistanceDataSources = "SplitterDistanceDataSources";
        static string _lastFile = "LastFile";
        static int _maxLastFiles = 10;

        // Constants for the encryption / decrpytion of the password for DB access
        static string _passPhrase = "Pas5pr@se";               // can be any string
        static string _saltValue = "s@1tValue";                // can be any string
        static string _hashAlgorithm = "SHA1";                 // can be "MD5"
        static int _passwordIterations = 2;                    // can be any number
        static string _initVector = "@1B2c3D4e5F6g7H8";        // must be 16 bytes
        static int _keySize = 256;                             // can be 192 or 128

        // Database connection registry constants
        static string _server = "Server";
        static string _database = "Database";
        static string _userName = "Username";
        static string _password = "Password";
        static string _winAuthentication = "WindowsAuthentication";
        static string _showAtStartup = "ShowAtStartup";
        static string _lastSavedSQLServer = "IsSQLServer";
        static string _accessDatabase = "AccessDatabase";
        static string _accessPassword = "AccessPassword";
        
        // Constant for the file name that we will save the configuration to
        static string _saveFile = "Mapster.map";
        static string _tableStructureFile = "MapsterSchema.xml";
        static string _msAccessOutput = "Mapster Data Upload.mdb";
        static string _msExcelOutput = "Mapster Data Upload.xlsx";
        static string _scriptFile = "Mapster Generated Script.sql";

        /// <summary>
        /// Constant to represent the application title.
        /// </summary>
        static string _kMapster = "Mapster ITC";

        // Highlight / no highlight colours for the buttons. These show which table we are
        // currently mapping.
        static Color _highlight = Color.DeepSkyBlue;
        static Color _noHighlight = Color.LightGray;
        static Color _disabled = Color.DarkGray;

        // Delimiter to use when concatenating fields (eg Lookup name etc)
        static char _reportDelimiter = '|';

        // Delimiter to use when concatenating fields (eg Lookup name etc)
        static char _concatDelimiter = '|';

        // Constants for selecting records based on time
        static string _oneDayAgo = "[Today - 1 day]";
        static string _oneWeekAgo = "[Today - 1 week]";
        static string _oneMonthAgo = "[Today - 1 month]";
        
        // Constant for defaulting a field mapping to todays date
        static string _today = "Today()";

        // Constant for defaulting a field mapping to a new GUID
        static string _newGUID = "New Guid()";

        // Constant for defaulting a field mapping to an autonumber sequence
        static string _autoSequence = "Auto sequence()";

        // Constant for defaulting a field mapping to "True"
        static string _trueValue = "True";

        // Constant for defaulting a field mapping to "False"
        static string _falseValue = "False";

        // Default value for the auto-built MS Access database. Gives us an idea of how old the
        // data is.
        static string _createdOn = "CreatedOn";

        // Default value for the auto-built MS Access database. Administration table.
        static string _adminTable = "Administration";

        static long _maxAccessDBSize = 2000000000;

        static string _logFile = "Mapster Auto Run Log.txt";

        // Constant for the auto-generated batch file name
        static string _batchFile = "Mapster.bat";

        // Constant for specifying custom lookup SQL
        static string _customLookupSQL = "@Custom SQL";

        // Constant for the error displayed when there is no table key defined
        static string _noTableKeyError = "This table does not have a key defined...cannot continue!";

        // Max length of text fields in an Access database.
        static int _accessMaxTextFieldLength = 8000;

        // Constant for specifying the name of the temp Mapster tables
        static string _tempTableExt = "_Mapster$";

        #endregion

        #region Properties

        /// <summary>
        /// Gets the registry folder.
        /// </summary>
        /// <value>The registry folder.</value>
        public static string RegistryFolder
        {
            get { return _registryFolder; }
        }

        /// <summary>
        /// Gets the window left.
        /// </summary>
        /// <value>The window left.</value>
        public static string WindowLeft
        {
            get { return _windowLeft; }
        }

        /// <summary>
        /// Gets the window top.
        /// </summary>
        /// <value>The window top.</value>
        public static string WindowTop
        {
            get { return _windowTop; }
        }

        /// <summary>
        /// Gets the width of the window.
        /// </summary>
        /// <value>The width of the window.</value>
        public static string WindowWidth
        {
            get { return _windowWidth; }
        }

        /// <summary>
        /// Gets the height of the window.
        /// </summary>
        /// <value>The height of the window.</value>
        public static string WindowHeight
        {
            get { return _windowHeight; }
        }

        /// <summary>
        /// Gets the state of the window.
        /// </summary>
        /// <value>The state of the window.</value>
        public static string WindowState
        {
            get { return _windowState; }
        }

        /// <summary>
        /// Gets the splitter sizing
        /// </summary>
        /// <value>The state of the window.</value>
        public static string SplitterDistance
        {
            get { return _splitterDistance; }
        }
        
        /// <summary>
        /// Gets the splitter sizing
        /// </summary>
        /// <value>The state of the window.</value>
        public static string SplitterDistanceDataSources
        {
            get { return _splitterDistanceDataSources; }
        }

        /// <summary>
        /// Gets the last file registry constant.
        /// </summary>
        /// <value>The state of the window.</value>
        public static string LastFile
        {
            get { return _lastFile; }
        }

        /// <summary>
        /// Gets the maximum number of files saved in the recent file list.
        /// </summary>
        /// <value>The state of the window.</value>
        public static int MaxLastFiles
        {
            get { return _maxLastFiles; }
        }
        
        /// <summary>
        /// Used for the Rijndael encryption algorithm
        /// </summary>
        public static string PassPhrase
        {
            get { return _passPhrase; }
        }

        /// <summary>
        /// Used for the Rijndael encryption algorithm
        /// </summary>
        public static string SaltValue
        {
            get { return _saltValue; }
        }

        /// <summary>
        /// Used for the Rijndael encryption algorithm
        /// </summary>
        public static string HashAlgorithm
        {
            get { return _hashAlgorithm; }
        }

        /// <summary>
        /// Used for the Rijndael encryption algorithm
        /// </summary>
        public static int PasswordIterations
        {
            get { return _passwordIterations; }
        }

        /// <summary>
        /// Used for the Rijndael encryption algorithm
        /// </summary>
        public static int KeySize
        {
            get { return _keySize; }
        }

        /// <summary>
        /// Used for the Rijndael encryption algorithm
        /// </summary>
        public static string InitVector
        {
            get { return _initVector; }
        }

        /// <summary>
        /// Gets the database.
        /// </summary>
        /// <value>The database.</value>
        public static string Database
        {
            get { return _database; }
        }

        /// <summary>
        /// Gets the server.
        /// </summary>
        /// <value>The server.</value>
        public static string Server
        {
            get { return _server; }
        }

        /// <summary>
        /// Gets the username.
        /// </summary>
        /// <value>The username.</value>
        public static string Username
        {
            get { return _userName; }
        }

        /// <summary>
        /// Gets the password.
        /// </summary>
        /// <value>The password.</value>
        public static string Password
        {
            get { return _password; }
        }

        /// <summary>
        /// Gets the windows authentication.
        /// </summary>
        /// <value>The windows authentication.</value>
        public static string WindowsAuthentication
        {
            get { return _winAuthentication; }
        }

        /// <summary>
        /// Gets the database path for the last Access build db.
        /// </summary>
        /// <value>The password.</value>
        public static string AccessDatabase
        {
            get { return _accessDatabase; }
        }

        /// <summary>
        /// Gets the password for the last Access build db.
        /// </summary>
        /// <value>The password.</value>
        public static string AccessPassword
        {
            get { return _accessPassword; }
        }

        /// <summary>
        /// Gets the show at startup.
        /// </summary>
        /// <value>The show at startup.</value>
        public static string ShowAtStartup
        {
            get { return _showAtStartup; }
        }

        /// <summary>
        /// Gets the show at startup.
        /// </summary>
        /// <value>The show at startup.</value>
        public static string LastSavedSQLServer
        {
            get { return _lastSavedSQLServer; }
        }

        /// <summary>
        /// Gets the name of the default XML configuration file
        /// </summary>
        public static string ConfigDefinitionFile
        {
            get { return _saveFile; }
        }

        /// <summary>
        /// Gets the name of the default batch file
        /// </summary>
        public static string BatchFile
        {
            get { return _batchFile; }
        }

        /// <summary>
        /// Gets the name of the default auto-generated script file
        /// </summary>
        public static string ScriptFile
        {
            get { return _scriptFile; }
        }

        /// <summary>
        /// Gets the name of the application
        /// </summary>
        public static string Mapster
        {
            get { return _kMapster; }
        }

        /// <summary>
        /// The expected output path for the Excel workbook!
        /// </summary>
        public static string MSExcelOutput
        {
            get
            {
                return _msExcelOutput;
            }
        }

        /// <summary>
        /// Administration table which we auto-add to the MS Access database.
        /// </summary>
        public static string AdminTable
        {
            get
            {
                return _adminTable;
            }
        }

        /// <summary>
        /// Auto timestamp-esqe to add to the auto-built MS Access tables
        /// </summary>
        public static string CreatedOn
        {
            get
            {
                return _createdOn;
            }
        }

        /// <summary>
        /// The expected output path for the Access database!
        /// </summary>
        public static string MSAccessOutput
        {
            get
            {
                return _msAccessOutput;
            }
        }

        /// <summary>
        /// The max file size for the Access database - Access will crash otherwise!
        /// </summary>
        public static long MaxAccessFileSize
        {
            get
            {
                return _maxAccessDBSize;
            }
        }
        /// <summary>
        /// We make a new log file for each day. Although we may only run the import
        /// once a day at the moment, in future this may change and then each log file
        /// will hold multiple run info.
        /// </summary>
        public static string TableStructureFile
        {
            get 
            {
                //string appPath = Path.GetDirectoryName(Application.ExecutablePath);

                //return appPath + "\\" + _tableStructureFile; 

                string appPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\Mapster";

                if (!Directory.Exists(appPath))
                {
                    Directory.CreateDirectory(appPath);
                }

                return appPath + "\\" + _tableStructureFile;                 
            }
        }

        /// <summary>
        /// Default delimiter used when concatenating several fields into one
        /// </summary>
        public static char ConcatenationDelimiter
        {
            get { return _concatDelimiter; }
        }

        /// <summary>
        /// Default delimiter used when concatenating report data
        /// </summary>
        public static char ReportDelimiter
        {
            get { return _reportDelimiter; }
        }

        /// <summary>
        /// Constant to use in the WHERE clause for the interface (ie select only those records that were updated
        /// one day ago)
        /// </summary>
        public static string OneDayAgo
        {
            get { return _oneDayAgo; }
        }

        /// <summary>
        /// Constant to use in the WHERE clause for the interface (ie select only those records that were updated
        /// one week ago)
        /// </summary>
        public static string OneWeekAgo
        {
            get { return _oneWeekAgo; }
        }

        /// <summary>
        /// Constant to use in the WHERE clause for the interface (ie select only those records that were updated
        /// one month ago)
        /// </summary>
        public static string OneMonthAgo
        {
            get { return _oneMonthAgo; }
        }

        /// <summary>
        /// Constant to use in defaulting a mapped field - sets the default to Todays date
        /// </summary>
        public static string Today
        {
            get { return _today; }
        }

        /// <summary>
        /// Constant to use in defaulting a mapped field - sets the default to create a new GUID
        /// Useful when scripting data etc!!
        /// </summary>
        public static string NewGUID
        {
            get { return _newGUID; }
        }
        
        /// <summary>
        /// Constant to use in defaulting a mapped field - sets the default to an auto-number
        /// </summary>
        public static string AutoSequence
        {
            get { return _autoSequence; }
        }

        /// <summary>
        /// Constant to use in defaulting a mapped field - sets the default to "True"
        /// </summary>
        public static string TrueValue
        {
            get { return _trueValue; }
        }

        /// <summary>
        /// Constant to use in defaulting a mapped field - sets the default to "False"
        /// </summary>
        public static string FalseValue
        {
            get { return _falseValue; }
        }

        /// <summary>
        /// Constant to use for the auto-run log file. Errors are recorded in the log file and then
        /// mailed to the administrator.
        /// </summary>
        public static string LogFile
        {
            get { return _logFile; }
        }

        /// <summary>
        /// Gets the constant to specify custom lookup SQL.
        /// </summary>
        /// <value>
        /// The custom lookup SQL.
        /// </value>
        public static string CustomLookupSQL
        {
            get { return _customLookupSQL; }
        }

        /// <summary>
        /// Gets the temporary table ext.
        /// </summary>
        /// <value>
        /// The temporary table ext.
        /// </value>
        public static string TempTableExt
        {
            get { return _tempTableExt; }
        }
        
        /// <summary>
        /// Gets the constant to display "no table key defined" error
        /// </summary>
        /// <value>
        /// The error message for when the user hasn't specified the key(s)
        /// </value>
        public static string NoTableKeyError
        {
            get { return _noTableKeyError; }
        }

        /// <summary>
        /// Gets the maximum length of a text field in an Access database.
        /// </summary>
        public static int AccessMaxTextFieldLength
        {
            get { return _accessMaxTextFieldLength; }
        }
        
        #endregion
    }
}
