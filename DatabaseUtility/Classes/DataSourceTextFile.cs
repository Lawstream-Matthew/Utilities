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
    /// This class represents a database system represented in a text file (e.g. CSV, pipe delim etc).
    /// </summary>
    [Serializable, DefaultPropertyAttribute("DatabaseType")]
    public class DataSourceTextFile : DataSourceBase
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
        /// Type of delimiter for the text file
        /// </summary>
        protected ConstantValues.VersionType _fileVersionType;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor - required for the XML deserialisation
        /// </summary>
        public DataSourceTextFile() : base()
        {
            _isDirty = false;
            
            _fileVersionType = ConstantValues.VersionType.CommaDelim;
            _fileNameAndPath = "";
            _fileName = "";
            _filePath = "";

            _connType = ConstantValues.ConnectionType.Text_File;
        }

        /// <summary>
        /// Additional constructor
        /// </summary>
        /// <param name="name">Name of the system that we will be working with. Must be unique.</param>
        public DataSourceTextFile(string name) : base(name)
        {
            _isDirty = false;

            _fileVersionType = ConstantValues.VersionType.CommaDelim;
            _fileNameAndPath = "";
            _fileName = "";
            _filePath = "";

            _connType = ConstantValues.ConnectionType.Text_File;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The directory to search for the text file data source.
        /// </summary>
        [XmlAttribute("SourceFilePath"), DisplayName("File path"), Category("Text Files"), DescriptionAttribute("The file name for the text file data source")]
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
        /// The type / version of the text or Excel file that we are using as a data source.
        /// </summary>
        [XmlAttribute("FileType"), DisplayName("File type"), Category("Text Files"), DescriptionAttribute("The type of text file format")]
        public ConstantValues.VersionType FileType
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
        /// Property to construct the connection string. The string returned will be catered to the
        /// connection type (SQL Server, Oracle OLE DB or Access OLE DB).
        /// </summary>
        [XmlIgnore(), BrowsableAttribute(false)]
        protected override string ConnectionString
        {
            get
            {
                string connStr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + _filePath + ";Extended Properties=\"text;HDR=Yes;FMT=Delimited\"";
                        
                return connStr;
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
                
                // Create the schema.ini file in the directory for this data source file if it
                // doesn't already exist. We need this to ensure that we can set the delimiter
                // in the data source file to the correct type (otherwise it'll default to CSV
                // and we can't read files delimited by other types). 
                // Check to see if the Schema.ini file need to be created
                if (this.CreateOrAddSchema())
                {
                    // Create a writer and open the file
                    TextWriter tw = new StreamWriter(this.SourceDirAndFile, true);

                    // Write the driver information to the Schema file.
                    tw.WriteLine("[" + _fileName + "]");
                    tw.WriteLine("ColNameHeader=True");
                    switch (this.FileType)
                    {
                        case ConstantValues.VersionType.CommaDelim:
                            tw.WriteLine("Format=CSVDelimited");
                            break;

                        case ConstantValues.VersionType.SemiColonDelim:
                            tw.WriteLine("Format=Delimited(;)");
                            break;

                        case ConstantValues.VersionType.TabDelim:
                            tw.WriteLine("Format=TabDelimited");
                            break;

                        case ConstantValues.VersionType.PipeDelim:
                            tw.WriteLine("Format=Delimited(|)");
                            break;
                    }

                    tw.WriteLine("MaxScanRows=0");
                    tw.WriteLine("CharacterSet=ANSI");

                    tw.Close();
                }
                if (_fileNameAndPath == "")
                {
                    valid = false;
                }
                
                return valid;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether or not we have to add an entry (or even create) the Schema entry
        /// for this particular data source text file. The OLEDB driver requires that the directory
        /// that contains the data source file has a Schema.ini file with format details in order to 
        /// be able to interpret the schema of the data source file.
        /// </summary>
        /// <returns></returns>
        private bool CreateOrAddSchema()
        {
            bool addEntry = true;
            string schemaFile = this.SourceDirAndFile;

            if (File.Exists(schemaFile))
            {
                // Create a reader and open the file
                TextReader tr = new StreamReader(schemaFile);
                try
                {
                    string text;
                    string compareTo = "[" + _fileName.ToLower() + "]";

                    while (tr.Peek() >= 0)
                    {
                        text = tr.ReadLine();
                        if (text.ToLower() == compareTo)
                        {
                            addEntry = false;
                            break;
                        }
                    }
                }
                finally
                {
                    tr.Close();
                }
            }

            return addEntry;
        }

        #endregion
    }
}
