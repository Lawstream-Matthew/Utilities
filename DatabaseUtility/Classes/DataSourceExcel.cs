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
    /// This class represents a database system in an Excel spreadsheet.
    /// </summary>
    [Serializable, DefaultPropertyAttribute("DatabaseType")]
    public class DataSourceExcel : DataSourceBase
    {
        #region Members

        // Text and Excel files
        /// <summary>
        /// Name of the Excel / Text file
        /// </summary>
        protected string _fileName;

        /// <summary>
        /// Path-on-disk to the Excel / Text file
        /// </summary>
        protected string _filePath;

        /// <summary>
        /// Concatenated file and path name on the disk
        /// </summary>
        protected string _fileNameAndPath;

        /// <summary>
        /// Type of Excel file - there's a diff in reading 2003 or 2007+ files
        /// </summary>
        protected ConstantValues.ExcelVersionType _fileVersionType;

        /// <summary>
        /// Name of the transformed workbook if we create a new one when doing the data transformation
        /// </summary>
        string _transformFilePath = "";

        #endregion
        
        #region Constructors

        /// <summary>
        /// Default constructor - required for the XML deserialisation
        /// </summary>
        public DataSourceExcel() : base()
        {
            _isDirty = false;
            _name = "";
           
            _fileVersionType = ConstantValues.ExcelVersionType.Excel2007;
            _fileNameAndPath = "";
            _fileName = "";
            _filePath = "";

            this.DatabaseType = ConstantValues.ConnectionType.MS_Excel;
            this.BracketChar = ConstantValues.BracketType.SquareBrackets;
        }

        /// <summary>
        /// Additional constructor
        /// </summary>
        /// <param name="name">Name of the system that we will be working with. Must be unique.</param>
        public DataSourceExcel(string name) : base(name)
        {
            _isDirty = false;

            _name = name;

            _fileVersionType = ConstantValues.ExcelVersionType.Excel2007;
            _fileNameAndPath = "";
            _fileName = "";
            _filePath = "";

            this.DatabaseType = ConstantValues.ConnectionType.MS_Excel;
            this.BracketChar = ConstantValues.BracketType.SquareBrackets;
        }
              

        #endregion

        #region Properties

        /// <summary>
        /// The directory to search for the text file data source.
        /// </summary>
        [XmlAttribute("SourceFilePath"), DisplayName("Excel file"), Category("Excel Files"), DescriptionAttribute("The file name for the Excel file data source")]
        public string SourceFilePath
        {
            get
            {
                return _fileNameAndPath;
            }
            set
            {
                _fileNameAndPath = value;

                // Break out the source file name from the full path. We do this as the text file
                // OLEDB driver and the Schema.ini file require both the path and file name
                // separately.
                if (value != "")
                {
                    _fileName = Path.GetFileName(_fileNameAndPath);
                    _filePath = Path.GetDirectoryName(_fileNameAndPath);
                }
                _isDirty = true;
            }
        }

        /// <summary>
        /// Property to construct the connection string. The string returned will be catered to the
        /// connection type (SQL Server, Oracle OLE DB or Access OLE DB).
        /// </summary>
        [XmlIgnore(), BrowsableAttribute(false)]
        protected override string ConnectionString
        {
            get
            {
                string connStr = "";

                if (this.FileType == ConstantValues.ExcelVersionType.Excel2003)
                {
                    connStr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + this.SourceFilePath + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=1\"";
                }
                else
                {
                    connStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + this.SourceFilePath + ";Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\"";
                }

                return connStr;
            }
        }

        /// <summary>
        /// The type / version of the text or Excel file that we are using as a data source.
        /// </summary>
        [XmlAttribute("FileType"), DisplayName("Excel file version"), Category("Excel Files"), DescriptionAttribute("The type of Excel file format")]
        public ConstantValues.ExcelVersionType FileType
        {
            get
            {
                return _fileVersionType;
            }
            set
            {
                _fileVersionType = value;
                _isDirty = true;
            }
        }

        /// <summary>
        /// The directory to search for the text file data source.
        /// </summary>
        [XmlIgnore(), BrowsableAttribute(false)]
        public string SourceDirAndFile
        {
            get
            {
                string schemaFile = _filePath + "\\Schema.ini";
                return schemaFile;
            }
        }

        /// <summary>
        /// The file name of the text / Excel file data source.
        /// </summary>
        [XmlIgnore(), BrowsableAttribute(false)]
        public string SourceFile
        {
            get
            {
                return _fileName;
            }
        }

        /// <summary>
        /// Property to determine whether or not the object has enough information to successfully
        /// try to connect to the server / database.
        /// </summary>
        [XmlIgnore(), BrowsableAttribute(false)]
        public override bool IsValid
        {
            get
            {
                bool valid = true;
            
                if (_fileNameAndPath == "")
                {
                    valid = false;
                }

                return valid;
            }
        }

        /// <summary>
        /// Name of the transformed workbook if we create a new one when doing the data transformation
        /// </summary>
        [XmlAttribute("TransformFilePath"), Browsable(false)]
        internal string TransformFilePath
        {
            get
            {
                string tempPath = _transformFilePath;
                if (this.FileType == ConstantValues.ExcelVersionType.Excel2003)
                {
                    tempPath = tempPath.Replace("xlsx", "xls");
                }

                return tempPath;
            }
            set
            {
                if (_transformFilePath != value)
                {
                    _transformFilePath = value;

                    this.IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Property to construct the connection string. The string returned will be catered to the
        /// connection type (SQL Server, Oracle OLE DB or Access OLE DB).
        /// </summary>
        [XmlIgnore(), BrowsableAttribute(false)]
        protected string ConnectionStringTransformFile
        {
            get
            {
                string connStr = "";

                if (this.FileType == ConstantValues.ExcelVersionType.Excel2003)
                {
                    // IMEX = 1 on this line tells Excel to treat mixed numeric / text data as text - BUT it stops us from writing to the file so leave it off here
                    connStr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + this.TransformFilePath + ";Extended Properties=\"Excel 8.0;HDR=Yes\"";
                }
                else
                {
                    // IMEX = 1 on this line tells Excel to treat mixed numeric / text data as text - BUT it stops us from writing to the file so leave it off here
                    connStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + this.TransformFilePath + ";Extended Properties=\"Excel 12.0 Xml;HDR=Yes\"";
                }

                return connStr;
            }
        }

        #endregion

        #region Methods
        #endregion
    }
}
