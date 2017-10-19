using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace LawstreamUpdate.Classes
{
    /// <summary>
    /// Interface for all the data source objects
    /// </summary>
    public interface IDataSource
    {                
        /// <summary>
        /// Opens the connection to the database based on the parameters that have been set 
        /// up by the user.
        /// </summary>
        /// <returns>IDBConnection object - use an interface object as we have different
        /// types of connection base objects, depending on the database type</returns>
        IDbConnection NewConnection();

        /// <summary>
        /// Function to return the column schema information for a given table. This will be used by
        /// this object as well as any inheriting objects. Note that the GetSchema function
        /// returns different types of information depending upon the database it is connected
        /// to so we try to stick to utilising only generic information. One notable item missing
        /// is any indication of the Primary Key - IMO a major overthought on the part of .NET.
        /// </summary>
        /// <param name="table">The table name.</param>
        /// <param name="colName">Name of the col.</param>
        /// <returns>
        /// A DataTable object containing the column schema defintions.
        /// </returns>
        DataTable ColumnSchema(string table, string colName = null);

        /// <summary>
        /// Retrieve the table schema from the database. This function also further splits
        /// itself into retrieving either the table information or view information, depending
        /// on the database source that has been defined for this system.
        /// </summary>
        /// <param name="showSchema">if set to <c>true</c> [show schema].</param>
        /// <param name="tblName">Name of the table.</param>
        /// <returns>
        /// A DataTable object containing the table schema defintions.
        /// </returns>
        DataTable TableSchema(bool showSchema, string tblName = "");
        
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
        IDataReader SelectData(string SQL, IDbConnection conn);
        
        /// <summary>
        /// Override implementation for the ExecuteSQL function that executes the 
        /// function without a Transaction object.
        /// </summary>
        /// <param name="SQL">The SQL statement to execute</param>
        /// <param name="executeScalar">Boolean flag indicating whether to execute as a scalar function</param>
        /// <returns></returns>
        int ExecuteSQL(string SQL, bool executeScalar);

        /// <summary>
        /// Executes an SQL statement against the database. The function will either run a "normal"
        /// execute function or a scalar execute function as requested. The scalar execute function
        /// is useful when we need to find out the identity field for the inserted record.
        /// </summary>
        /// <param name="SQL">The SQL statement to execute</param>
        /// <param name="trans">A transaction object to attach to the command as required</param>
        /// <param name="executeScalar">Boolean flag indicating whether to execute as a scalar function</param>
        /// <returns></returns>
        int ExecuteSQL(string SQL, IDbTransaction trans, bool executeScalar);
       
        /// <summary>
        /// Function to test the connection parameters to ensure that we have set up a valid 
        /// connection to the system.
        /// </summary>
        /// <returns>True if the connection attempt succeeded, False if it did not.</returns>
        bool TestConnection(out string error);
        
    }
}
