using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Data.SqlClient;
using System.IO;
using System.Data;
using System.Text.RegularExpressions;

namespace LawstreamUpdate.Classes
{
    /// <summary>
    /// Does whatever work we're looking at doing for this interation of the utility. We've separated it out on it's
    /// own so that we can easily replace it for different jobs
    /// </summary>
    public class DoWork
    {
        public delegate void ProgressUpdateHandler(ProgressUpdateEventArgs e);

        public event ProgressUpdateHandler OnProgressUpdate;

        #region Properties

        /// <summary>
        /// Gets or sets the folder path for the data files.
        /// </summary>
        /// <value>
        /// The folder path.
        /// </value>
        public string FolderPath
        {
            get;
            set;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Extracts the document links.
        /// </summary>
        /// <param name="dbConn">The database connection.</param>
        /// <param name="recCount">The record count.</param>
        /// <param name="updateCount">The update count.</param>
        /// <exception cref="Exception"></exception>
        public void ExtractDocumentLinksWithLegislation(DBConnection dbConn, out int recCount)
        {
            recCount = 0;

            StringBuilder SQL = new StringBuilder();
            SQL.AppendLine("SELECT COUNT(*) OVER () AS TotalRecords, x.* FROM (");
            SQL.AppendLine("SELECT m.[ID] AS [MappingID], m.[DetailMarkup] AS [Markup], ");
            SQL.AppendLine("ld.[ID] AS [LegislationID], ld.[Name] AS [Legislation], lm.[ID] AS [Legislation Subsection ID], lm.[Name] AS [Legislation Subsection], ");
            SQL.AppendLine("c.[ID] AS [CompanyID], c.[Name] AS [Company], cp.[ID] AS [ProjectID], cp.[Name] AS [Project] ");
            SQL.AppendLine("FROM [MappingProject] m ");
            SQL.AppendLine("LEFT JOIN Legislation l ON l.ID = m.LegislationID ");
            SQL.AppendLine("LEFT JOIN LegislationDefinition ld ON ld.ID = l.LegislationDefinitionID  ");
            SQL.AppendLine("LEFT JOIN LegislationMetadata lm ON lm.ID = l.LegislationMetadataID ");
            SQL.AppendLine("LEFT JOIN [CompanyProject] cp ON cp.ID = m.CompanyProjectID ");
            SQL.AppendLine("LEFT JOIN [Company] c ON c.[ID] = cp.[CompanyID] ");
            SQL.AppendLine("WHERE cp.[CompanyID] IN ");
            SQL.AppendLine("(SELECT [ID] FROM [Company] where [Name] like 'clough%') ");
            SQL.AppendLine(" AND (LOWER(m.[DetailMarkup]) like '%href%')");

            SQL.AppendLine("");
            SQL.AppendLine("UNION ALL");
            SQL.AppendLine("");

            SQL.AppendLine("SELECT m.[ID] AS [MappingID], m.[DetailMarkup] AS [Markup], ");
            SQL.AppendLine("ld.[ID] AS [LegislationID], ld.[Name] AS [Legislation], lm.[ID] AS [Legislation Subsection ID], lm.[Name] AS [Legislation Subsection], ");
            SQL.AppendLine("c.[ID] AS [CompanyID], c.[Name] AS [Company], NULL AS [ProjectID], NULL AS [Project] ");
            SQL.AppendLine("FROM [Mapping] m ");
            SQL.AppendLine("LEFT JOIN Legislation l ON l.ID = m.LegislationID ");
            SQL.AppendLine("LEFT JOIN LegislationDefinition ld ON ld.ID = l.LegislationDefinitionID  ");
            SQL.AppendLine("LEFT JOIN LegislationMetadata lm ON lm.ID = l.LegislationMetadataID ");
            SQL.AppendLine("LEFT JOIN[User] u ON u.ID = m.UserID ");
            SQL.AppendLine("LEFT JOIN [Company] c ON c.[ID] = u.[CompanyID] ");
            SQL.AppendLine("WHERE u.CompanyID IN ");
            SQL.AppendLine("    (SELECT [ID] FROM [Company] where [Name] like 'clough%') ");
            SQL.AppendLine("AND (LOWER(m.[DetailMarkup]) like '%href%')");
            SQL.AppendLine(") as x");
            SQL.AppendLine("ORDER BY [Company], [Project], [Legislation], [Legislation Subsection] ");

            try
            {
                dbConn.Connection.Open();

                // Select all of the Distinct values in the source table and add
                // them to the translations collection. Later we will get the edits
                // and display them.
                List<DocumentData> documentDataList = new List<DocumentData>();
                using (SqlDataReader dr = dbConn.SelectData(SQL.ToString()))
                {
                    while (dr.Read())
                    {
                        recCount++;

                        int totalRecordCount = dr.GetInt32(dr.GetOrdinal("TotalRecords"));
                        int mappingID = dr.GetInt32(dr.GetOrdinal("MappingID"));
                        string detailMarkup = dr.GetString(dr.GetOrdinal("Markup"));
                        int legislationID = dr.GetInt32(dr.GetOrdinal("LegislationID"));
                        string legislationName = dr.GetString(dr.GetOrdinal("Legislation"));
                        int legislationSectionID = dr.GetInt32(dr.GetOrdinal("Legislation Subsection ID"));
                        string legislationSectionName = dr.GetString(dr.GetOrdinal("Legislation Subsection"));
                        int companyID = dr.GetInt32(dr.GetOrdinal("CompanyID"));
                        string companyName = dr.GetString(dr.GetOrdinal("Company"));

                        Guid? projectID = null;
                        string projectName = string.Empty;
                        if (!dr.IsDBNull(dr.GetOrdinal("ProjectID")))
                        {
                            projectID = dr.GetGuid(dr.GetOrdinal("ProjectID"));
                            projectName = dr.GetString(dr.GetOrdinal("Project"));
                        }

                        if (recCount % 5 == 0)
                        {
                            if (OnProgressUpdate != null)
                            {
                                ProgressUpdateEventArgs temp = new ProgressUpdateEventArgs(recCount, totalRecordCount);
                                OnProgressUpdate(temp);
                            }
                        }

                        // Find / create the Legislation item that this belongs to
                        DocumentData documentDataItem = documentDataList.Find(x => ((x.CompanyID == companyID) && (((projectID.HasValue) && (x.ProjectID == projectID)))
                                                                                    && (x.LegislationID == legislationID) && (x.LegislationSubsectionID == legislationSectionID)));

                        if (documentDataItem == null)
                        {
                            documentDataItem = new DocumentData();
                            documentDataItem.CompanyID = companyID;
                            documentDataItem.Company = companyName;
                            documentDataItem.ProjectID = projectID;
                            documentDataItem.Project = projectName;
                            documentDataItem.LegislationID = legislationID;
                            documentDataItem.Legislation = legislationName;
                            documentDataItem.LegislationSubsectionID = legislationSectionID;
                            documentDataItem.LegislationSubsection = legislationSectionName.Replace("\n", " ").Replace(",", " /");
                            documentDataItem.MappingID = mappingID;

                            documentDataList.Add(documentDataItem);
                        }

                        // Extract every "href" segment in the detail markup
                        int startIndex = detailMarkup.IndexOf("href=\"", 0);
                        while (startIndex != -1)
                        {
                            int newSearchPosn = startIndex + 6;
                            int endIndex = detailMarkup.IndexOf("\"", newSearchPosn);

                            string documentURL = detailMarkup.Substring(newSearchPosn, (endIndex - newSearchPosn)).Trim();

                            if ((!documentURL.ToLower().StartsWith("mailto")) && (documentURL.ToLower().StartsWith("http")))
                            {
                                // Grab the document name
                                int startDocumentNamePosn = detailMarkup.IndexOf(">", endIndex);
                                int endDocumentNamePosn = detailMarkup.IndexOf("</a>", startDocumentNamePosn);

                                // Check for a <span tag
                                int startSpanTag = detailMarkup.IndexOf("<span", startDocumentNamePosn);
                                if ((startSpanTag > 0) && (startSpanTag < endDocumentNamePosn))
                                {
                                    // We have a span tag, document name is within that
                                    // Find the end of the span tag
                                    startDocumentNamePosn = detailMarkup.IndexOf(">", startSpanTag);
                                    endDocumentNamePosn = detailMarkup.IndexOf("</span", startSpanTag);
                                }

                                endDocumentNamePosn--;
                                string documentName = detailMarkup.Substring(startDocumentNamePosn + 1, (endDocumentNamePosn - startDocumentNamePosn)).Trim();

                                // Some doc names ended in a <br /> tag? 
                                documentName = documentName.Replace("<br />", "");

                                if (!documentDataItem.DocumentList.Contains(new KeyValuePair<string, string>(documentName, documentURL)))
                                {
                                    documentDataItem.DocumentList.Add(new KeyValuePair<string, string>(documentName, documentURL));

                                    documentDataItem.DocumentList = documentDataItem.DocumentList.OrderBy(x => x.Key).ToList();
                                }
                            }

                            startIndex = startIndex = detailMarkup.IndexOf("href=\"", endIndex);
                        }
                    }

                    // Sort before writing out to the CSV file
                    documentDataList = documentDataList.OrderBy(a => a.Company).ThenBy(b => b.Project).ThenBy(c => c.Legislation).ThenBy(d => d.LegislationSubsection).ToList();

                    // Dump out to a file
                    using (FileStream fs = new FileStream(this.FolderPath + @"\Clough Files - all.csv", FileMode.Create, FileAccess.Write))
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        string headerLine = "Company, Project, Legislation, Legislation Subsection, Document, URL, MappingID";
                        sw.WriteLine(headerLine);

                        foreach (DocumentData docDataItem in documentDataList)
                        {
                            foreach (KeyValuePair<string, string> docPair in docDataItem.DocumentList)
                            {
                                string updateDocURL = docDataItem.Company + "," + docDataItem.Project + "," + docDataItem.Legislation + "," + docDataItem.LegislationSubsection + ",";

                                if (docPair.Key.Contains(","))
                                {
                                    updateDocURL += "\"" + docPair.Key + "\",";
                                }
                                else
                                {
                                    updateDocURL += docPair.Key + ",";
                                }
                              
                                string cleanedURL = docPair.Value.Replace("%20", " ").Replace("%2F", "/").Replace("%2f", "/").Replace("%2D", "-").Replace("%2d", "-").Replace("&amp;", "&");
                                cleanedURL = cleanedURL.Replace("%2E", ".").Replace("%2e", ".").Replace("%3A", ":").Replace("%3a", ":");

                                updateDocURL += cleanedURL + "," + docDataItem.MappingID.ToString();
                                sw.WriteLine(updateDocURL);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                dbConn.Connection.Close();
            }
        }

        /// <summary>
        /// Extracts the document links.
        /// </summary>
        /// <param name="dbConn">The database connection.</param>
        /// <param name="recCount">The record count.</param>
        /// <param name="updateCount">The update count.</param>
        /// <exception cref="Exception"></exception>
        public void ExtractUniqueDocumentLinks(DBConnection dbConn, out int recCount, bool includeInvalidDocumentNames = false)
        {
            recCount = 0;

            StringBuilder SQL = new StringBuilder();
            SQL.AppendLine("SELECT COUNT(*) OVER () AS TotalRecords, x.* FROM (");
            SQL.AppendLine("SELECT m.[ID] AS [MappingID], m.[DetailMarkup] AS [Markup] ");
            SQL.AppendLine("FROM [MappingProject] m ");
            SQL.AppendLine("LEFT JOIN [CompanyProject] cp ON cp.ID = m.CompanyProjectID ");
            SQL.AppendLine("LEFT JOIN [Company] c ON c.[ID] = cp.[CompanyID] ");
            SQL.AppendLine("WHERE cp.[CompanyID] IN ");
            SQL.AppendLine("(SELECT [ID] FROM [Company] where [Name] like 'clough%') ");
            SQL.AppendLine(" AND (LOWER(m.[DetailMarkup]) like '%href%')");

            SQL.AppendLine("");
            SQL.AppendLine("UNION ALL");
            SQL.AppendLine("");

            SQL.AppendLine("SELECT m.[ID] AS [MappingID], m.[DetailMarkup] AS [Markup] ");
            SQL.AppendLine("FROM [Mapping] m ");
            SQL.AppendLine("LEFT JOIN[User] u ON u.ID = m.UserID ");
            SQL.AppendLine("LEFT JOIN [Company] c ON c.[ID] = u.[CompanyID] ");
            SQL.AppendLine("WHERE u.CompanyID IN ");
            SQL.AppendLine("    (SELECT [ID] FROM [Company] where [Name] like 'clough%') ");
            SQL.AppendLine("AND (LOWER(m.[DetailMarkup]) like '%href%')");
            SQL.AppendLine(") as x");

            try
            {
                dbConn.Connection.Open();

                // Select all of the Distinct values in the source table and add
                // them to the translations collection. Later we will get the edits
                // and display them.
                List<DocumentData> documentDataList = new List<DocumentData>();
                DocumentData documentDataItem = new DocumentData();
                documentDataList.Add(documentDataItem);

                using (SqlDataReader dr = dbConn.SelectData(SQL.ToString()))
                {
                    while (dr.Read())
                    {
                        recCount++;

                        int totalRecordCount = dr.GetInt32(dr.GetOrdinal("TotalRecords"));
                        int mappingID = dr.GetInt32(dr.GetOrdinal("MappingID"));
                        string detailMarkup = dr.GetString(dr.GetOrdinal("Markup"));

                        if (recCount % 5 == 0)
                        {
                            if (OnProgressUpdate != null)
                            {
                                ProgressUpdateEventArgs temp = new ProgressUpdateEventArgs(recCount, totalRecordCount);
                                OnProgressUpdate(temp);
                            }
                        }

                        // Extract every "href" segment in the detail markup
                        int startIndex = detailMarkup.IndexOf("href=\"", 0);
                        while (startIndex != -1)
                        {
                            int newSearchPosn = startIndex + 6;
                            int endIndex = detailMarkup.IndexOf("\"", newSearchPosn);

                            string documentURL = detailMarkup.Substring(newSearchPosn, (endIndex - newSearchPosn)).Trim();

                            if ((!documentURL.ToLower().StartsWith("mailto")) && (documentURL.ToLower().StartsWith("http")))
                            {
                                // Grab the document name
                                int startDocumentNamePosn = detailMarkup.IndexOf(">", endIndex);
                                int endDocumentNamePosn = detailMarkup.IndexOf("</a>", startDocumentNamePosn);

                                // Check for a <span tag
                                int startSpanTag = detailMarkup.IndexOf("<span", startDocumentNamePosn);
                                if ((startSpanTag > 0) && (startSpanTag < endDocumentNamePosn))
                                {
                                    // We have a span tag, document name is within that
                                    // Find the end of the span tag
                                    startDocumentNamePosn = detailMarkup.IndexOf(">", startSpanTag);
                                    endDocumentNamePosn = detailMarkup.IndexOf("</span", startSpanTag);
                                }

                                endDocumentNamePosn--;
                                string documentName = detailMarkup.Substring(startDocumentNamePosn + 1, (endDocumentNamePosn - startDocumentNamePosn)).Trim();

                                // Some doc names ended in a <br /> tag? And some have a comma.
                                documentName = documentName.Replace("<br />", "");

                                if (!includeInvalidDocumentNames)
                                {
                                    documentName = documentName.Replace("&nbsp;", "");

                                    if (string.IsNullOrWhiteSpace(documentName))
                                    {
                                        startIndex = startIndex = detailMarkup.IndexOf("href=\"", endIndex);
                                        continue;
                                    }
                                }

                                if (!documentDataItem.DocumentList.Contains(new KeyValuePair<string, string>(documentName, string.Empty)))
                                {
                                    documentDataItem.DocumentList.Add(new KeyValuePair<string, string>(documentName, string.Empty));

                                    documentDataItem.DocumentList = documentDataItem.DocumentList.OrderBy(x => x.Key).ToList();
                                }
                            }

                            startIndex = startIndex = detailMarkup.IndexOf("href=\"", endIndex);
                        }
                    }

                    // Dump out to a file
                    using (FileStream fs = new FileStream(this.FolderPath + @"\Clough Files - unique.csv", FileMode.Create, FileAccess.Write))
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        string headerLine = "Document, URL";
                        sw.WriteLine(headerLine);

                        foreach (DocumentData docDataItem in documentDataList)
                        {
                            foreach (KeyValuePair<string, string> docPair in docDataItem.DocumentList)
                            {
                                string updateDocURL = string.Empty; ;

                                if (docPair.Key.Contains(","))
                                {
                                    updateDocURL = "\"" + docPair.Key + "\",";
                                }
                                else
                                {
                                    updateDocURL = docPair.Key + ",";
                                }

                                string cleanedURL = docPair.Value.Replace("%20", " ").Replace("%2F", "/").Replace("%2f", "/").Replace("%2D", "-").Replace("%2d", "-").Replace("&amp;", "&");
                                cleanedURL = cleanedURL.Replace("%2E", ".").Replace("%2e", ".").Replace("%3A", ":").Replace("%3a", ":");

                                updateDocURL += cleanedURL;
                                sw.WriteLine(updateDocURL);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                dbConn.Connection.Close();
            }
        }

        /// <summary>
        /// Extracts the document links.
        /// </summary>
        /// <param name="dbConn">The database connection.</param>
        /// <param name="recCount">The record count.</param>
        /// <param name="updateCount">The update count.</param>
        /// <exception cref="Exception"></exception>
        public void ExtractInValidDocumentLinksWithLegislation(DBConnection dbConn, out int recCount)
        {
            recCount = 0;

            StringBuilder SQL = new StringBuilder();
            SQL.AppendLine("SELECT COUNT(*) AS TotalRecords, * FROM (");
            SQL.AppendLine("SELECT m.[ID] AS [MappingID], m.[DetailMarkup] AS [Markup], ");
            SQL.AppendLine("ld.[ID] AS [LegislationID], ld.[Name] AS [Legislation], lm.[ID] AS [Legislation Subsection ID], lm.[Name] AS [Legislation Subsection], ");
            SQL.AppendLine("c.[ID] AS [CompanyID], c.[Name] AS [Company], cp.[ID] AS [ProjectID], cp.[Name] AS [Project] ");
            SQL.AppendLine("FROM [MappingProject] m ");
            SQL.AppendLine("LEFT JOIN Legislation l ON l.ID = m.LegislationID ");
            SQL.AppendLine("LEFT JOIN LegislationDefinition ld ON ld.ID = l.LegislationDefinitionID  ");
            SQL.AppendLine("LEFT JOIN LegislationMetadata lm ON lm.ID = l.LegislationMetadataID ");
            SQL.AppendLine("LEFT JOIN [CompanyProject] cp ON cp.ID = m.CompanyProjectID ");
            SQL.AppendLine("LEFT JOIN [Company] c ON c.[ID] = cp.[CompanyID] ");
            SQL.AppendLine("WHERE cp.[CompanyID] IN ");
            SQL.AppendLine("(SELECT [ID] FROM [Company] where [Name] like 'clough%') ");
            SQL.AppendLine(" AND (LOWER(m.[DetailMarkup]) like '%href%')");

            SQL.AppendLine("");
            SQL.AppendLine("UNION ALL");
            SQL.AppendLine("");

            SQL.AppendLine("SELECT m.[ID] AS [MappingID], m.[DetailMarkup] AS [Markup], ");
            SQL.AppendLine("ld.[ID] AS [LegislationID], ld.[Name] AS [Legislation], lm.[ID] AS [Legislation Subsection ID], lm.[Name] AS [Legislation Subsection], ");
            SQL.AppendLine("c.[ID] AS [CompanyID], c.[Name] AS [Company], NULL AS [ProjectID], NULL AS [Project] ");
            SQL.AppendLine("FROM [Mapping] m ");
            SQL.AppendLine("LEFT JOIN Legislation l ON l.ID = m.LegislationID ");
            SQL.AppendLine("LEFT JOIN LegislationDefinition ld ON ld.ID = l.LegislationDefinitionID  ");
            SQL.AppendLine("LEFT JOIN LegislationMetadata lm ON lm.ID = l.LegislationMetadataID ");
            SQL.AppendLine("LEFT JOIN[User] u ON u.ID = m.UserID ");
            SQL.AppendLine("LEFT JOIN [Company] c ON c.[ID] = u.[CompanyID] ");
            SQL.AppendLine("WHERE u.CompanyID IN ");
            SQL.AppendLine("    (SELECT [ID] FROM [Company] where [Name] like 'clough%') ");
            SQL.AppendLine("AND (LOWER(m.[DetailMarkup]) like '%href%')");
            SQL.AppendLine("ORDER BY [Company], [Project], [Legislation], [Legislation Subsection] ");
            SQL.AppendLine(")");

            try
            {
                dbConn.Connection.Open();

                // Select all of the Distinct values in the source table and add
                // them to the translations collection. Later we will get the edits
                // and display them.
                List<DocumentData> documentDataList = new List<DocumentData>();
                using (SqlDataReader dr = dbConn.SelectData(SQL.ToString()))
                {
                    while (dr.Read())
                    {
                        recCount++;

                        int totalRecordCount = dr.GetInt32(dr.GetOrdinal("TotalRecords"));
                        int mappingID = dr.GetInt32(dr.GetOrdinal("MappingID"));
                        string detailMarkup = dr.GetString(dr.GetOrdinal("Markup"));
                        int legislationID = dr.GetInt32(dr.GetOrdinal("LegislationID"));
                        string legislationName = dr.GetString(dr.GetOrdinal("Legislation"));
                        int legislationSectionID = dr.GetInt32(dr.GetOrdinal("Legislation Subsection ID"));
                        string legislationSectionName = dr.GetString(dr.GetOrdinal("Legislation Subsection"));
                        int companyID = dr.GetInt32(dr.GetOrdinal("CompanyID"));
                        string companyName = dr.GetString(dr.GetOrdinal("Company"));

                        Guid? projectID = null;
                        string projectName = string.Empty;
                        if (!dr.IsDBNull(dr.GetOrdinal("ProjectID")))
                        {
                            projectID = dr.GetGuid(dr.GetOrdinal("ProjectID"));
                            projectName = dr.GetString(dr.GetOrdinal("Project"));
                        }

                        if (recCount % 5 == 0)
                        {
                            if (OnProgressUpdate != null)
                            {
                                ProgressUpdateEventArgs temp = new ProgressUpdateEventArgs(recCount, totalRecordCount);
                                OnProgressUpdate(temp);
                            }
                        }

                        // Find / create the Legislation item that this belongs to
                        DocumentData documentDataItem = documentDataList.Find(x => ((x.CompanyID == companyID) && (((projectID.HasValue) && (x.ProjectID == projectID)))
                                                                                    && (x.LegislationID == legislationID) && (x.LegislationSubsectionID == legislationSectionID)));

                        if (documentDataItem == null)
                        {
                            documentDataItem = new DocumentData();
                            documentDataItem.MappingID = mappingID;
                            documentDataItem.CompanyID = companyID;
                            documentDataItem.Company = companyName;
                            documentDataItem.ProjectID = projectID;
                            documentDataItem.Project = projectName;
                            documentDataItem.LegislationID = legislationID;
                            documentDataItem.Legislation = legislationName;
                            documentDataItem.LegislationSubsectionID = legislationSectionID;
                            documentDataItem.LegislationSubsection = legislationSectionName.Replace("\n", " ").Replace(",", " /");
                        }

                        // Extract every "href" segment in the detail markup
                        int startIndex = detailMarkup.IndexOf("href=\"", 0);

                        bool addedItem = false;
                        while (startIndex != -1)
                        {
                            int newSearchPosn = startIndex + 6;
                            int endIndex = detailMarkup.IndexOf("\"", newSearchPosn);

                            string documentURL = detailMarkup.Substring(newSearchPosn, (endIndex - newSearchPosn)).Trim();

                            if ((!documentURL.ToLower().StartsWith("mailto")) && (documentURL.ToLower().StartsWith("http")))
                            {
                                // Grab the document name
                                int startDocumentNamePosn = detailMarkup.IndexOf(">", endIndex);
                                int endDocumentNamePosn = detailMarkup.IndexOf("</a>", startDocumentNamePosn);

                                int endLookupURLPosn = endDocumentNamePosn;

                                // Check for a <span tag
                                int startSpanTag = detailMarkup.IndexOf("<span", startDocumentNamePosn);
                                if ((startSpanTag > 0) && (startSpanTag < endDocumentNamePosn))
                                {
                                    // We have a span tag, document name is within that
                                    // Find the end of the span tag
                                    startDocumentNamePosn = detailMarkup.IndexOf(">", startSpanTag);
                                    endDocumentNamePosn = detailMarkup.IndexOf("</span", startSpanTag);
                                }

                                endDocumentNamePosn--;
                                string documentName = detailMarkup.Substring(startDocumentNamePosn + 1, (endDocumentNamePosn - startDocumentNamePosn)).Trim();

                                // Some doc names ended in a <br /> tag? And some have a comma.
                                documentName = documentName.Replace("<br />", "").Replace(",", " /");

                                bool addDocument = false;

                                int docAsInt;
                                string lookupURL = string.Empty;
                                if ((documentName == "&nbsp;") || (string.IsNullOrWhiteSpace(documentName)) || (string.IsNullOrWhiteSpace(documentURL)))
                                {
                                    string prevPartOfMarkup = detailMarkup.Substring(0, startIndex);
                                    int lookupStart = prevPartOfMarkup.LastIndexOf("<a");

                                    // Extract everything from <a....href> to </a>. These will be removed from the mapping
                                    lookupURL = detailMarkup.Substring(lookupStart, ((endLookupURLPosn + 4) - lookupStart)).Trim();

                                    addDocument = true;
                                }
                                else if (Int32.TryParse(documentName, out docAsInt))
                                {
                                    string prevPartOfMarkup = detailMarkup.Substring(0, startIndex);
                                    int lookupStart = prevPartOfMarkup.LastIndexOf("<a");

                                    // Extract everything from <a....href> to </a>. These will be removed from the mapping
                                    lookupURL = detailMarkup.Substring(lookupStart, ((endLookupURLPosn + 4) - lookupStart)).Trim();

                                    addDocument = true;
                                }

                                if (addDocument)
                                {
                                    if (!documentDataItem.DocumentList.Contains(new KeyValuePair<string, string>(documentName, documentURL + "|" + lookupURL)))
                                    {
                                        documentDataItem.DocumentList.Add(new KeyValuePair<string, string>(documentName, documentURL + "|" + lookupURL));

                                        documentDataItem.DocumentList = documentDataItem.DocumentList.OrderBy(x => x.Key).ToList();
                                    }

                                    if (!addedItem)
                                    {
                                        documentDataList.Add(documentDataItem);

                                        addedItem = true;
                                    }
                                }
                            }

                            startIndex = startIndex = detailMarkup.IndexOf("href=\"", endIndex);
                        }
                    }

                    // Sort before writing out to the CSV file
                    documentDataList = documentDataList.OrderBy(a => a.Company).ThenBy(b => b.Project).ThenBy(c => c.Legislation).ThenBy(d => d.LegislationSubsection).ToList();

                    // Dump out to a file
                    using (FileStream fs = new FileStream(this.FolderPath + @"\Clough Files - Invalid documents.csv", FileMode.Create, FileAccess.Write))
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        string headerLine = "Mapping ID,Company,Project,Legislation,Legislation Subsection,Document,URL,LookupURL";
                        sw.WriteLine(headerLine);

                        foreach (DocumentData docDataItem in documentDataList)
                        {
                            foreach (KeyValuePair<string, string> docPair in docDataItem.DocumentList)
                            {
                                string updateDocURL = docDataItem.MappingID + "," + docDataItem.Company + "," + docDataItem.Project + "," + docDataItem.Legislation + "," + docDataItem.LegislationSubsection + "," + docPair.Key + ",";

                                string[] splitURL = docPair.Value.Split('|');

                                string cleanedURL = splitURL[0].Replace("%20", " ").Replace("%2F", "/").Replace("%2f", "/").Replace("%2D", "-").Replace("%2d", "-").Replace("&amp;", "&");
                                cleanedURL = cleanedURL.Replace("%2E", ".").Replace("%2e", ".").Replace("%3A", ":").Replace("%3a", ":");

                                cleanedURL = cleanedURL + "," + splitURL[1].Replace("\n", "' + Char(10) + '");
                                updateDocURL += cleanedURL;
                                sw.WriteLine(updateDocURL);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                dbConn.Connection.Close();
            }
        }

        /// <summary>
        /// Replaces the document links.
        /// </summary>
        /// <param name="dbConn">The database connection.</param>
        /// <param name="recCount">The record count.</param>
        /// <param name="updateCount">The update count.</param>
        /// <exception cref="Exception"></exception>
        public void RemoveDocumentLinks(DBConnection dbConn, out int recCount, out int updateCount)
        {
            List<string> fileReplacements = new List<string>();

            recCount = 0;
            updateCount = 0;

            // Read the file, pick out the LookupURLK column number
            int lookupURLColumn = -1;
            using (FileStream fs = new FileStream(this.FolderPath + @"\Clough Files - Invalid documents.csv", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    // Find the LookupURL column
                    string readText = sr.ReadLine();

                    string[] fileValues = readText.Split(',');
                    if (lookupURLColumn < 0)
                    {
                        for (int i = 0; i <= fileValues.GetUpperBound(0); i++)
                        {
                            if (fileValues[i].ToLower().Trim() == "lookupurl")
                            {
                                lookupURLColumn = i;
                                break;
                            }
                        }

                        if (lookupURLColumn < 0)
                        {
                            throw new Exception("Couldn't find the LookupURL column!");
                        }
                    }

                    while (!sr.EndOfStream)
                    {
                        readText = sr.ReadLine();

                        // Set up the replacements
                        fileValues = readText.Split(',');

                        if (!fileReplacements.Contains(fileValues[lookupURLColumn]))
                        {
                            fileReplacements.Add(fileValues[lookupURLColumn]);
                        }
                    }
                }
            }

            // Now go and do the actual replacements
            StringBuilder SQL = new StringBuilder();
            SQL.AppendLine("UPDATE Mapping SET DetailMarkup = REPLACE(DetailMarkup, @URL, '')");
            SQL.AppendLine("WHERE ID IN");
            SQL.AppendLine("(");
            SQL.AppendLine("SELECT [ID] FROM Mapping ");
            SQL.AppendLine("WHERE DetailMarkup like '%' + @URL + '%'");
            SQL.AppendLine(");");

            SQL.AppendLine("UPDATE MappingProject SET DetailMarkup = REPLACE(DetailMarkup, @URL, '')");
            SQL.AppendLine("WHERE ID IN");
            SQL.AppendLine("(");
            SQL.AppendLine("SELECT [ID] FROM MappingProject ");
            SQL.AppendLine("WHERE DetailMarkup like '%' + @URL + '%'");
            SQL.AppendLine(")");

            try
            {
                dbConn.Connection.Open();

                // Select all of the Distinct values in the source table and add
                // them to the translations collection. Later we will get the edits
                // and display them.
                SqlCommand cmd = dbConn.Connection.CreateCommand();
                foreach (string entry in fileReplacements)
                {
                    recCount++;

                    if (recCount % 5 == 0)
                    {
                        if (OnProgressUpdate != null)
                        {
                            ProgressUpdateEventArgs temp = new ProgressUpdateEventArgs(recCount, fileReplacements.Count);
                            OnProgressUpdate(temp);
                        }
                    }

                    cmd.Parameters.Clear();
                    if (entry.Contains("' + Char(10) + '"))
                    {
                        cmd.CommandText = SQL.ToString().Replace("@URL", "'" + entry + "'");
                    }
                    else
                    {
                        cmd.CommandText = SQL.ToString();
                        cmd.CommandType = System.Data.CommandType.Text;

                        SqlParameter param = cmd.Parameters.Add("@URL", System.Data.SqlDbType.NVarChar, -1);
                        param.Value = entry;
                    }

                    int updated = cmd.ExecuteNonQuery();

                    updateCount += updated;
                }

                updateCount += RemoveInvalidURLsInCode(dbConn, false, false, out recCount);
                updateCount += RemoveInvalidURLsInCode(dbConn, true, false, out recCount);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                dbConn.Connection.Close();
            }
        }

        public void CleanUpLeftoverURLs(DBConnection dbConn, out int recCount, out int updateCount)
        {
            recCount = 0;
            updateCount = 0;
            try
            {
                dbConn.Connection.Open();

                updateCount += RemoveInvalidURLsInCode(dbConn, false, true, out recCount);
                updateCount += RemoveInvalidURLsInCode(dbConn, true, true, out recCount);
            
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                dbConn.Connection.Close();
            }
        }

        private int RemoveInvalidURLsInCode(DBConnection dbConn, bool isProject, bool postRunCleanUp, out int recCount)
        {
            int updatedCount = 0;
            recCount = 0;

            // Now go and do the actual replacements
            StringBuilder SQL = new StringBuilder();
            StringBuilder updateSQL = new StringBuilder();

            if (!isProject)
            {
                SQL.AppendLine("SELECT COUNT(*) OVER () AS TotalRecords, m.[ID] AS [MappingID], m.[Detail] AS [Detail], m.[DetailMarkup] AS [Markup] ");
                SQL.AppendLine("FROM [Mapping] m ");
                SQL.AppendLine("LEFT JOIN[User] u ON u.ID = m.UserID ");
                SQL.AppendLine("LEFT JOIN [Company] c ON c.[ID] = u.[CompanyID] ");
                SQL.AppendLine("WHERE u.CompanyID IN ");
                SQL.AppendLine("    (SELECT [ID] FROM [Company] where [Name] like 'clough%') ");

                if (postRunCleanUp)
                {
                    SQL.AppendLine(" AND (LOWER(m.[DetailMarkup]) like '%</a>%')");
                }
                else
                {
                    SQL.AppendLine(" AND (LOWER(m.[DetailMarkup]) like '%href%')");
                }

          //      SQL.AppendLine("AND m.[ID] = 6844");

                updateSQL.AppendLine("UPDATE Mapping SET DetailMarkup = @DetailMarkup, ChangedOn = GETDATE(), ChangedBy = 'Lawstream Admin'");
                updateSQL.AppendLine("WHERE ID = @MappingID");
            }
            else
            {
                SQL.AppendLine("SELECT COUNT(*) OVER () AS TotalRecords,m.[ID] AS [MappingID], m.[Detail] AS [Detail], m.[DetailMarkup] AS [Markup] ");
                SQL.AppendLine("FROM [MappingProject] m ");
                SQL.AppendLine("LEFT JOIN [CompanyProject] cp ON cp.ID = m.CompanyProjectID ");
                SQL.AppendLine("LEFT JOIN [Company] c ON c.[ID] = cp.[CompanyID] ");
                SQL.AppendLine("WHERE cp.[CompanyID] IN ");
                SQL.AppendLine("(SELECT [ID] FROM [Company] where [Name] like 'clough%') ");
                if (postRunCleanUp)
                {
                    SQL.AppendLine(" AND (LOWER(m.[DetailMarkup]) like '%</a>%')");
                }
                else
                {
                    SQL.AppendLine(" AND (LOWER(m.[DetailMarkup]) like '%href%')");
                }
                    //  SQL.AppendLine("AND m.[ID] = 4780");

                updateSQL.AppendLine("UPDATE MappingProject SET DetailMarkup = @DetailMarkup, ChangedOn = GETDATE(), ChangedBy = 'Lawstream Admin'");
                updateSQL.AppendLine("WHERE ID = @MappingID");
            }

            try
            {
                //dbConn.Connection.Open();

                List<SqlCommand> updateCommandList = new List<SqlCommand>();
                using (SqlTransaction sqlTrans = dbConn.Connection.BeginTransaction("DocumentRegisterTransaction"))
                {
                    try
                    {
                        SqlCommand cmd = dbConn.Connection.CreateCommand();
                        cmd.CommandText = SQL.ToString();
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.Transaction = sqlTrans;

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                try
                                {
                                    int totalRecords = dr.GetInt32(dr.GetOrdinal("TotalRecords"));
                                    int mappingID = dr.GetInt32(dr.GetOrdinal("MappingID"));
                                    string detailMarkup = dr.GetString(dr.GetOrdinal("Markup"));

                                    recCount++;

                                    if (recCount % 5 == 0)
                                    {
                                        if (OnProgressUpdate != null)
                                        {
                                            ProgressUpdateEventArgs temp = new ProgressUpdateEventArgs(recCount, totalRecords);
                                            OnProgressUpdate(temp);
                                        }
                                    }

                                    if (postRunCleanUp)
                                    {
                                        // Now find any orphaned <\a> items, yes they DO exist!!
                                        int closeLinkPosn = detailMarkup.ToLower().IndexOf("</a>");
                                        while (closeLinkPosn != -1)
                                        {
                                            if (closeLinkPosn != -1)
                                            {
                                                int linkStart = detailMarkup.ToLower().IndexOf("<a", 0);
                                                if (linkStart == -1)
                                                {
                                                    // Remove the orphaned </a> text
                                                    string firstPart = detailMarkup.Substring(0, closeLinkPosn);
                                                    string secondPart = detailMarkup.Substring(closeLinkPosn + 4, detailMarkup.Length - (closeLinkPosn + 4));
                                                    detailMarkup = firstPart + secondPart;
                                                }
                                                else
                                                {
                                                    break;
                                                }
                                            }

                                            closeLinkPosn = detailMarkup.ToLower().IndexOf("</a>", closeLinkPosn);
                                        }
                                    }
                                    else
                                    {
                                        int linkStart = detailMarkup.ToLower().IndexOf("<a");
                                        while (linkStart != -1)
                                        {
                                            if (linkStart > -1)
                                            {
                                                int endLinkPosn = detailMarkup.ToLower().IndexOf("</a>", linkStart);

                                                if (endLinkPosn == -1)
                                                {
                                                    // Remove the dead link
                                                    int nextLinkStartPosn = detailMarkup.ToLower().IndexOf("<a", linkStart);
                                                    string hrefLink = detailMarkup.Substring(linkStart, nextLinkStartPosn - linkStart).Trim();
                                                    detailMarkup = detailMarkup.Replace(hrefLink, "");

                                                    updatedCount++;
                                                }
                                                else
                                                {
                                                    int nextLinkStartPosn = detailMarkup.ToLower().IndexOf("<a", linkStart + 1);
                                                    if ((nextLinkStartPosn > -1) && (nextLinkStartPosn < endLinkPosn))
                                                    {
                                                        // Remove the dead link
                                                        endLinkPosn = detailMarkup.ToLower().IndexOf(">", linkStart);
                                                        string hrefLink = detailMarkup.Substring(linkStart, (endLinkPosn + 1) - linkStart).Trim();
                                                        detailMarkup = detailMarkup.Replace(hrefLink, "");

                                                        updatedCount++;
                                                    }
                                                }

                                                linkStart = detailMarkup.ToLower().IndexOf("<a", endLinkPosn);
                                            }
                                        }
                                    }

                                    // Collect the update commands
                                    SqlCommand updateCmd = dbConn.Connection.CreateCommand();
                                    updateCmd.CommandText = updateSQL.ToString();
                                    updateCmd.CommandType = System.Data.CommandType.Text;
                                    updateCmd.Transaction = sqlTrans;

                                    SqlParameter detailMarkupParam = updateCmd.Parameters.Add("@DetailMarkup", System.Data.SqlDbType.NVarChar, -1);
                                    SqlParameter idParam = updateCmd.Parameters.Add("@MappingID", System.Data.SqlDbType.Int);

                                    detailMarkupParam.Value = detailMarkup;
                                    idParam.Value = mappingID;

                                    updateCommandList.Add(updateCmd);
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception(ex.Message);
                                }
                            }
                        }

                        // Now fire off the SQL commands
                        foreach (SqlCommand updateCmd in updateCommandList)
                        {
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        sqlTrans.Rollback();
                        throw new Exception(ex.Message);
                    }

                    sqlTrans.Commit();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                //dbConn.Connection.Close();
            }

            return updatedCount;
        }

        /// <summary>
        /// Fixes the "correct" document names as sent back by Clough. Cos they were all over the show!!
        /// </summary>
        /// <param name="dbConn">The database connection.</param>
        /// <param name="recCount">The record count.</param>
        /// <param name="updateCount">The update count.</param>
        /// <exception cref="Exception"></exception>
        public void FixDocumentNames(out int recCount, out int updateCount)
        {
            int totalCount = 0;
            recCount = 0;
            updateCount = 0;

            DataSourceTextFile dataSource = new DataSourceTextFile();

            string filePath = this.FolderPath + @"\Clough URL Changes.csv";
            string destFilePath = this.FolderPath + @"\Clough URL Changes Fixed.csv";
            string errorFilePath = this.FolderPath + @"\Clough URL Changes Errors.csv";

            try
            {
                // Count the lines in the file
                using (StreamReader r = new StreamReader(filePath))
                {
                    int i = 0;
                    while (r.ReadLine() != null) { totalCount++; }
                }

                // Use SQL to read and update the file
                dataSource.DatabaseType = ConstantValues.ConnectionType.Text_File;
                dataSource.FileType = ConstantValues.VersionType.CommaDelim;
                dataSource.SourceFilePath = filePath;

                string SQL = "SELECT * FROM [" + dataSource.SourceFile + "]";

                using (FileStream outFS = new FileStream(destFilePath, FileMode.Create, FileAccess.Write))
                {
                    using (FileStream outErrorFS = new FileStream(errorFilePath, FileMode.Create, FileAccess.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(outFS))
                        {
                            using (StreamWriter swError = new StreamWriter(outErrorFS))
                            {
                                // Write out the header line
                                sw.WriteLine("Company,Document,Existing URL,Correct Document Name,Correct URL");
                                swError.WriteLine("Company,Document,Existing URL,Correct Document Name,Correct URL");

                                // Read the file, find the headers we need
                                using (IDbConnection connSource = dataSource.NewConnection())
                                {
                                    using (IDataReader dr = dataSource.SelectData(SQL, connSource))
                                    {
                                        try
                                        {
                                            while (dr.Read())
                                            {
                                                bool hasError = false;

                                                string company = dr.GetString(dr.GetOrdinal("Company"));

                                                string existingDocument = string.Empty;
                                                try
                                                {
                                                    existingDocument = dr.GetString(dr.GetOrdinal("Document"));
                                                }
                                                catch
                                                {
                                                    hasError = true;
                                                }

                                                string existingURL = string.Empty;
                                                try
                                                {
                                                    existingURL = dr.GetString(dr.GetOrdinal("Existing URL"));
                                                }
                                                catch
                                                {
                                                    // Not really an error, we don't use this anyways
                                                }

                                                string correctDoc = string.Empty;
                                                try
                                                {
                                                    correctDoc = dr.GetString(dr.GetOrdinal("Correct Document Name"));
                                                }
                                                catch
                                                {
                                                    hasError = true;
                                                }

                                                string correctURL = string.Empty;
                                                try
                                                {
                                                    correctURL = dr.GetString(dr.GetOrdinal("Correct URL"));
                                                }
                                                catch
                                                {
                                                    hasError = true;
                                                }

                                                // Some of the "CORP" parts have spaces where they shouldn't.
                                                // Or don't have the "-" between the doc name and "CORP".
                                                // Fix on the fly.
                                                int corpIndex = correctDoc.IndexOf("CORP");
                                                if (corpIndex > 0)
                                                {
                                                    string corpSub = correctDoc.Substring(corpIndex, correctDoc.Length - corpIndex).Trim();

                                                    if (correctDoc.Contains(" CORP - "))
                                                    {
                                                        corpSub = corpSub.Replace(" - ", "-");
                                                    }

                                                    if (correctDoc.Contains(" - CORP"))
                                                    {
                                                        // Rebuild the string after correction
                                                        correctDoc = correctDoc.Substring(0, corpIndex - 2).Trim() + " " + corpSub;
                                                        updateCount++;
                                                    }
                                                    else
                                                    {
                                                        // Rebuild the string after correction
                                                        correctDoc = correctDoc.Substring(0, corpIndex - 1).Trim() + " " + corpSub;
                                                        updateCount++;
                                                    }
                                                }

                                                string insertData = company + ", ";

                                                if (existingDocument.Contains(","))
                                                {
                                                    insertData += "\"" + existingDocument + "\",";
                                                }
                                                else
                                                {
                                                    insertData += existingDocument + ",";
                                                }

                                                if (existingURL.Contains(","))
                                                {
                                                    insertData += "\"" + existingURL + "\",";
                                                }
                                                else
                                                {
                                                    insertData += existingURL + ",";
                                                }

                                                if (correctDoc.Contains(","))
                                                {
                                                    insertData += "\"" + correctDoc + "\",";
                                                }
                                                else
                                                {
                                                    insertData += correctDoc + ",";
                                                }

                                                if (correctURL.Contains(","))
                                                {
                                                    insertData += "\"" + correctURL;
                                                }
                                                else
                                                {
                                                    insertData += correctURL;
                                                }

                                                if (hasError)
                                                {
                                                    swError.WriteLine(insertData);
                                                }
                                                else
                                                {
                                                    sw.WriteLine(insertData);
                                                }

                                                recCount++;
                                                if (recCount % 5 == 0)
                                                {
                                                    if (OnProgressUpdate != null)
                                                    {
                                                        ProgressUpdateEventArgs temp = new ProgressUpdateEventArgs(recCount, totalCount);
                                                        OnProgressUpdate(temp);
                                                    }
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            throw new Exception(ex.Message);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        ///// <summary>
        ///// Replaces the document links.
        ///// </summary>
        ///// <param name="dbConn">The database connection.</param>
        ///// <param name="recCount">The record count.</param>
        ///// <param name="updateCount">The update count.</param>
        ///// <exception cref="Exception"></exception>
        //public void UpdateDocumentLinks(DBConnection dbConn, out int recCount, out int updateCount)
        //{
        //    List<DocumentURL> fileReplacements = new List<DocumentURL>();

        //    recCount = 0;
        //    updateCount = 0;
        //    StringBuilder updateSQL = new StringBuilder(1000);

        //    // Read the file, find the headers we need
        //    int legislationColumn = -1;
        //    int legislationMetadataColumn = -1;
        //    int projectColumn = -1;
        //    int docColumn = -1;
        //    int existingURLColumn = -1;
        //    int correctDocNameColumn = -1;
        //    int correctURLColumn = -1;
        //    using (FileStream fs = new FileStream(this.FolderPath + @"\Clough URL Changes Fixed.csv", FileMode.Open, FileAccess.Read))
        //    {
        //        using (StreamReader sr = new StreamReader(fs))
        //        {
        //            // Pick up the columns we need
        //            string readText = sr.ReadLine();
        //            string[] fileValues = readText.Split(',');

        //            for (int i = 0; i <= fileValues.GetUpperBound(0); i++)
        //            {
        //                if (fileValues[i].ToLower().Trim() == "document")
        //                {
        //                    docColumn = i;
        //                }
        //                else if (fileValues[i].ToLower().Trim() == "existing url")
        //                {
        //                    existingURLColumn = i;
        //                }
        //                else if (fileValues[i].ToLower().Trim() == "correct document name")
        //                {
        //                    correctDocNameColumn = i;
        //                }
        //                else if (fileValues[i].ToLower().Trim() == "correct url")
        //                {
        //                    correctURLColumn = i;
        //                }
        //                else if (fileValues[i].ToLower().Trim() == "legislation")
        //                {
        //                    legislationColumn = i;
        //                }
        //                else if (fileValues[i].ToLower().Trim() == "legislation subsection")
        //                {
        //                    legislationMetadataColumn = i;
        //                }
        //                else if (fileValues[i].ToLower().Trim() == "project")
        //                {
        //                    projectColumn = i;
        //                }
        //            }

        //            if ((docColumn < 0) || (existingURLColumn < 0) || (correctDocNameColumn < 0) || (correctURLColumn < 0)
        //                || (legislationColumn < 0) || (legislationMetadataColumn < 0) || (projectColumn < 0))
        //            {
        //                throw new Exception("Couldn't find the all the columns!");
        //            }

        //            while (!sr.EndOfStream)
        //            {
        //                readText = sr.ReadLine();

        //                // Set up the replacements
        //                fileValues = readText.Split(',');

        //                DocumentURL docURL = new DocumentURL();
        //                docURL.ExistingDoc = fileValues[docColumn];
        //                docURL.ExistingURL = fileValues[existingURLColumn];
        //                docURL.CorrectDoc = fileValues[correctDocNameColumn];
        //                docURL.CorrectURL = fileValues[correctURLColumn];
        //                docURL.Legislation = fileValues[legislationColumn];
        //                docURL.LegislationMetadata = fileValues[legislationMetadataColumn];
        //                docURL.Project = fileValues[projectColumn];

        //                DocumentURL test = fileReplacements.Find(x => x.ExistingDoc == docURL.ExistingDoc
        //                        && x.Legislation == docURL.Legislation && x.LegislationMetadata == docURL.LegislationMetadata
        //                        && x.Project == docURL.Project);

        //                if (test == null)
        //                {
        //                    fileReplacements.Add(docURL);
        //                }
        //            }
        //        }
        //    }


        //    // Now go and do the actual replacements
        //    StringBuilder SQL = new StringBuilder();
        //    SQL.AppendLine("UPDATE Mapping SET DetailMarkup = REPLACE(DetailMarkup, @ExistingHref, @ReplacementHref)");
        //    SQL.AppendLine("WHERE ID = @MappingID");

        //    StringBuilder projectSQL = new StringBuilder();
        //    projectSQL.AppendLine("UPDATE MappingProject SET DetailMarkup = REPLACE(DetailMarkup, @ExistingHref, @ReplacementHref)");
        //    projectSQL.AppendLine("WHERE ID = @MappingID");

        //    try
        //    {
        //        dbConn.Connection.Open();

        //        using (SqlTransaction sqlTrans = dbConn.Connection.BeginTransaction("SampleTransaction"))
        //        {
        //            try
        //            {

        //                // Build up the existing and replacement HTML.
        //                SqlCommand cmd = dbConn.Connection.CreateCommand();
        //                cmd.CommandText = SQL.ToString();
        //                cmd.CommandType = System.Data.CommandType.Text;
        //                cmd.Transaction = sqlTrans;

        //                SqlParameter existingHrefParam = cmd.Parameters.Add("@ExistingHref", System.Data.SqlDbType.NVarChar, -1);
        //                SqlParameter replacementHrefParam = cmd.Parameters.Add("@ReplacementHref", System.Data.SqlDbType.NVarChar, -1);
        //                SqlParameter idParam = cmd.Parameters.Add("@MappingID", System.Data.SqlDbType.Int);

        //                // Build up the existing and replacement HTML.
        //                SqlCommand projectCmd = dbConn.Connection.CreateCommand();
        //                projectCmd.CommandText = projectSQL.ToString();
        //                projectCmd.CommandType = System.Data.CommandType.Text;
        //                projectCmd.Transaction = sqlTrans;

        //                SqlParameter existingHrefProjectParam = projectCmd.Parameters.Add("@ExistingHref", System.Data.SqlDbType.NVarChar, -1);
        //                SqlParameter replacementHrefProjectParam = projectCmd.Parameters.Add("@ReplacementHref", System.Data.SqlDbType.NVarChar, -1);
        //                SqlParameter idProjectParam = projectCmd.Parameters.Add("@MappingID", System.Data.SqlDbType.Int);

        //                foreach (DocumentURL entry in fileReplacements)
        //                {
        //                    recCount++;

        //                    if (recCount % 5 == 0)
        //                    {
        //                        if (OnProgressUpdate != null)
        //                        {
        //                            ProgressUpdateEventArgs temp = new ProgressUpdateEventArgs(recCount, fileReplacements.Count);
        //                            OnProgressUpdate(temp);
        //                        }
        //                    }

        //                    // Build up the existing <a href...>Doc</a> string and it's replacement. Then replace via t-sql.
        //                    Dictionary<int, string> existHrefList = GetDocumentAndLink(dbConn, sqlTrans, entry.Legislation, entry.LegislationMetadata, entry.Project, entry.ExistingDoc);

        //                    string replacementHref = "<a target=\"blank\" href=\"" + entry.CorrectURL + ">" + entry.CorrectDoc + "</a>";

        //                    if (string.IsNullOrWhiteSpace(entry.Project))
        //                    {
        //                        replacementHrefParam.Value = replacementHref;
        //                    }
        //                    else
        //                    {
        //                        replacementHrefProjectParam.Value = replacementHref;
        //                    }

        //                    foreach (KeyValuePair<int, string> hrefLink in existHrefList)
        //                    {

        //                        if (replacementHref != hrefLink.Value)
        //                        {
        //                            if (string.IsNullOrWhiteSpace(entry.Project))
        //                            {
        //                                idParam.Value = hrefLink.Key;
        //                                existingHrefParam.Value = hrefLink.Value;

        //                                int updated = cmd.ExecuteNonQuery();
        //                                updateCount += updated;
        //                            }
        //                            else
        //                            {
        //                                idProjectParam.Value = hrefLink.Key;
        //                                existingHrefProjectParam.Value = hrefLink.Value;

        //                                int updated = projectCmd.ExecuteNonQuery();
        //                                updateCount += updated;
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                sqlTrans.Rollback();
        //                throw new Exception(ex.Message);
        //            }

        //            sqlTrans.Commit();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //    finally
        //    {
        //        dbConn.Connection.Close();
        //    }
        //}

        /// <summary>
        /// Gets the document and link.
        /// </summary>
        /// <param name="dbConn">The database connection.</param>
        /// <param name="sqlTrans">The SQL trans.</param>
        /// <param name="legislationName">Name of the legislation.</param>
        /// <param name="legislationMetadataName">Name of the legislation metadata.</param>
        /// <param name="project">The project.</param>
        /// <param name="existingDoc">The existing document.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private Dictionary<int, string> GetDocumentAndLink(DBConnection dbConn, SqlTransaction sqlTrans, string existingDoc, bool isProject)
        {
            Dictionary<int, string> hrefList = new Dictionary<int, string>();
            StringBuilder SQL = new StringBuilder();

            if (isProject)
            {
                SQL.AppendLine("SELECT m.[ID] AS [MappingID], m.[DetailMarkup] AS [Markup], ");
                SQL.AppendLine("ld.[ID] AS [LegislationID], ld.[Name] AS [Legislation], lm.[ID] AS [Legislation Subsection ID], lm.[Name] AS [Legislation Subsection], ");
                SQL.AppendLine("c.[ID] AS [CompanyID], c.[Name] AS [Company], cp.[ID] AS [ProjectID], cp.[Name] AS [Project] ");
                SQL.AppendLine("FROM [MappingProject] m ");
                SQL.AppendLine("LEFT JOIN Legislation l ON l.ID = m.LegislationID ");
                SQL.AppendLine("LEFT JOIN LegislationDefinition ld ON ld.ID = l.LegislationDefinitionID  ");
                SQL.AppendLine("LEFT JOIN LegislationMetadata lm ON lm.ID = l.LegislationMetadataID ");
                SQL.AppendLine("LEFT JOIN [CompanyProject] cp ON cp.ID = m.CompanyProjectID ");
                SQL.AppendLine("LEFT JOIN [Company] c ON c.[ID] = cp.[CompanyID] ");
                SQL.AppendLine("WHERE cp.[CompanyID] IN ");
                SQL.AppendLine("(SELECT [ID] FROM [Company] where [Name] like 'clough%') ");
                SQL.AppendLine("AND ((m.[DetailMarkup] LIKE '%" + existingDoc.Replace("'", "''") + "%')");
                SQL.AppendLine("    OR (m.[DetailMarkup] LIKE '%" + existingDoc.Replace("'", "''").Replace(", ", " / ") + "%'))");
            }
            else
            {
                SQL.AppendLine("SELECT m.[ID] AS [MappingID], m.[DetailMarkup] AS [Markup], ");
                SQL.AppendLine("ld.[ID] AS [LegislationID], ld.[Name] AS [Legislation], lm.[ID] AS [Legislation Subsection ID], lm.[Name] AS [Legislation Subsection], ");
                SQL.AppendLine("c.[ID] AS [CompanyID], c.[Name] AS [Company], NULL AS [ProjectID], NULL AS [Project] ");
                SQL.AppendLine("FROM [Mapping] m ");
                SQL.AppendLine("LEFT JOIN Legislation l ON l.ID = m.LegislationID ");
                SQL.AppendLine("LEFT JOIN LegislationDefinition ld ON ld.ID = l.LegislationDefinitionID  ");
                SQL.AppendLine("LEFT JOIN LegislationMetadata lm ON lm.ID = l.LegislationMetadataID ");
                SQL.AppendLine("LEFT JOIN[User] u ON u.ID = m.UserID ");
                SQL.AppendLine("LEFT JOIN [Company] c ON c.[ID] = u.[CompanyID] ");
                SQL.AppendLine("WHERE u.CompanyID IN ");
                SQL.AppendLine("    (SELECT [ID] FROM [Company] where [Name] like 'clough%') ");
                SQL.AppendLine("AND ((m.[DetailMarkup] LIKE '%" + existingDoc.Replace("'", "''") + "%')");
                SQL.AppendLine("    OR (m.[DetailMarkup] LIKE '%" + existingDoc.Replace("'", "''").Replace(", ", " / ") + "%'))");
            }

            try
            {
                SqlCommand cmd = dbConn.Connection.CreateCommand();
                cmd.CommandText = SQL.ToString();
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Transaction = sqlTrans;

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        int mappingID = dr.GetInt32(dr.GetOrdinal("MappingID"));
                        string detailMarkup = dr.GetString(dr.GetOrdinal("Markup"));

                        // Extract every "href" segment in the detail markup
                        int startIndex = detailMarkup.IndexOf(">" + existingDoc + "<");

                        if (startIndex > 0)
                        {
                            // Now find the "<a" start posn
                            string prevPartOfMarkup = detailMarkup.Substring(0, startIndex);

                            int linkStart = prevPartOfMarkup.LastIndexOf("<a");
                            int endLinkPosn = detailMarkup.IndexOf("</a>", startIndex);

                            if ((linkStart > -1) && (endLinkPosn > -1))
                            {
                                if (endLinkPosn > linkStart)
                                {
                                    string hrefLink = detailMarkup.Substring(linkStart, ((endLinkPosn + 4) - linkStart)).Trim();

                                    hrefList.Add(mappingID, hrefLink);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return hrefList;
        }

        /// <summary>
        /// Validates the documents and urls. Looks for duplicates!
        /// </summary>
        /// <param name="docCount">The document count.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Couldn't find the all the columns!</exception>
        public List<Tuple<string, string>> ValidateDocumentsAndUrls(out int docCount, out int nonDuplicateCount)
        {
            List<DocumentURL> fileReplacements = new List<DocumentURL>();
            List<Tuple<string, string>> duplicateDocuments = new List<Tuple<string, string>>();

            docCount = 0;
            nonDuplicateCount = 0;

            // Read the file, find the headers we need
            int docColumn = -1;
            int existingURLColumn = -1;
            int correctDocNameColumn = -1;
            int correctURLColumn = -1;
            using (FileStream fs = new FileStream(this.FolderPath + @"\Clough URL Changes Fixed.csv", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    // Pick up the columns we need
                    string readText = sr.ReadLine();
                    string[] fileValues = readText.Split(',');

                    for (int i = 0; i <= fileValues.GetUpperBound(0); i++)
                    {
                        if (fileValues[i].ToLower().Trim() == "document")
                        {
                            docColumn = i;
                        }
                        else if (fileValues[i].ToLower().Trim() == "existing url")
                        {
                            existingURLColumn = i;
                        }
                        else if (fileValues[i].ToLower().Trim() == "correct document name")
                        {
                            correctDocNameColumn = i;
                        }
                        else if (fileValues[i].ToLower().Trim() == "correct url")
                        {
                            correctURLColumn = i;
                        }
                    }

                    if ((docColumn < 0) || (existingURLColumn < 0) || (correctDocNameColumn < 0) || (correctURLColumn < 0))
                    {
                        throw new Exception("Couldn't find the all the columns!");
                    }

                    while (!sr.EndOfStream)
                    {
                        readText = sr.ReadLine();

                        // Set up the replacements
                        fileValues = readText.Split(',');

                        DocumentURL docURL = new DocumentURL();
                        docURL.ExistingDoc = fileValues[docColumn];
                        docURL.ExistingURL = fileValues[existingURLColumn];
                        docURL.CorrectDoc = fileValues[correctDocNameColumn];
                        docURL.CorrectURL = fileValues[correctURLColumn];

                        DocumentURL test = fileReplacements.Find(x => x.CorrectDoc == docURL.CorrectDoc);

                        if (test == null)
                        {
                            docCount++;
                            nonDuplicateCount++;

                            fileReplacements.Add(docURL);
                        }
                        else
                        {
                            docCount++;

                            // Check to see if the URL is the same. Otherwise we have a problem...
                            if (test.CorrectURL != docURL.CorrectURL)
                            {
                                // Has this original item already been recorded (eg we may be > 1 duplicate)?
                                Tuple<string, string> originalDoc = duplicateDocuments.Find(x => x.Item1 == test.CorrectDoc && x.Item2 == test.CorrectURL);
                                if (originalDoc == null)
                                {
                                    duplicateDocuments.Add(new Tuple<string, string>(test.CorrectDoc, test.CorrectURL));
                                }

                                // Record this doc / URL combination
                                originalDoc = duplicateDocuments.Find(x => x.Item1 == docURL.CorrectDoc && x.Item2 == docURL.CorrectURL);
                                if (originalDoc == null)
                                {
                                    duplicateDocuments.Add(new Tuple<string, string>(docURL.CorrectDoc, docURL.CorrectURL));
                                }
                            }
                        }
                    }
                }
            }

            return duplicateDocuments;
        }

        /// <summary>
        /// Updates the URLs in the database with the registered document markup.
        /// </summary>
        /// <param name="dbConn">The database connection.</param>
        /// <param name="recCount">The record count.</param>
        /// <param name="updateCount">The update count.</param>
        /// <exception cref="Exception">
        /// Couldn't find the all the columns!
        /// or
        /// or
        /// </exception>
        public void UpdateExistingURLsInCode(DBConnection dbConn, out int recCount, out int updateCount)
        {
            List<DocumentURL> fileReplacements = new List<DocumentURL>();

            recCount = 0;
            updateCount = 0;
            StringBuilder updateSQL = new StringBuilder(1000);

            DataSourceTextFile dataSource = new DataSourceTextFile();
            string sourceFilePath = this.FolderPath + @"\Clough URL Changes Fixed.csv";

            try
            {
                // Use SQL to read and update the file
                dataSource.DatabaseType = ConstantValues.ConnectionType.Text_File;
                dataSource.FileType = ConstantValues.VersionType.CommaDelim;
                dataSource.SourceFilePath = sourceFilePath;

                string selectSQL = "SELECT * FROM [" + dataSource.SourceFile + "]";

                // Read the file, process
                using (IDbConnection connSource = dataSource.NewConnection())
                {
                    using (IDataReader dr = dataSource.SelectData(selectSQL, connSource))
                    {
                        try
                        {
                            while (dr.Read())
                            {
                                string existingDocument = dr.GetString(dr.GetOrdinal("Document"));

                                string existingURL = string.Empty;
                                if (!dr.IsDBNull(dr.GetOrdinal("Existing URL")))
                                {
                                    existingURL = dr.GetString(dr.GetOrdinal("Existing URL"));
                                }

                                string correctDoc = dr.GetString(dr.GetOrdinal("Correct Document Name"));
                                string correctURL = dr.GetString(dr.GetOrdinal("Correct URL"));

                                DocumentURL docURL = new DocumentURL();
                                docURL.ExistingDoc = existingDocument.Trim();
                                docURL.ExistingURL = existingURL.Trim();
                                docURL.CorrectDoc = correctDoc.Trim();
                                docURL.CorrectURL = correctURL.Trim();

                                DocumentURL test = fileReplacements.Find(x => x.ExistingDoc == docURL.ExistingDoc);

                                if (test == null)
                                {
                                    fileReplacements.Add(docURL);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                    }
                }

                int tempRecCount = 0;
                updateCount += UpdateCorrectURLsInCode(dbConn, fileReplacements, false, out tempRecCount);
                recCount += tempRecCount;

                updateCount += UpdateCorrectURLsInCode(dbConn, fileReplacements, true, out tempRecCount);
                recCount += tempRecCount;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private int UpdateCorrectURLsInCode(DBConnection dbConn, List<DocumentURL> fileReplacements, bool isProject, out int recCount)
        {
            int updatedCount = 0;
            recCount = 0;

            // Now go and do the actual replacements
            StringBuilder SQL = new StringBuilder();
            StringBuilder updateSQL = new StringBuilder();

            if (!isProject)
            {
                SQL.AppendLine("SELECT COUNT(*) OVER () AS TotalRecords, m.[ID] AS [MappingID], m.[Detail] AS [Detail], m.[DetailMarkup] AS [Markup] ");
                SQL.AppendLine("FROM [Mapping] m ");
                SQL.AppendLine("LEFT JOIN[User] u ON u.ID = m.UserID ");
                SQL.AppendLine("LEFT JOIN [Company] c ON c.[ID] = u.[CompanyID] ");
                SQL.AppendLine("WHERE u.CompanyID IN ");
                SQL.AppendLine("    (SELECT [ID] FROM [Company] where [Name] like 'clough%') ");
                SQL.AppendLine(" AND (LOWER(m.[DetailMarkup]) like '%href%')");
       //         SQL.AppendLine("AND m.[ID] = 35846");

                updateSQL.AppendLine("UPDATE Mapping SET Detail = @Detail, DetailMarkup = @DetailMarkup, ChangedOn = GETDATE(), ChangedBy = 'Lawstream Admin'");
                updateSQL.AppendLine("WHERE ID = @MappingID");
            }
            else
            {
                SQL.AppendLine("SELECT COUNT(*) OVER () AS TotalRecords,m.[ID] AS [MappingID], m.[Detail] AS [Detail], m.[DetailMarkup] AS [Markup] ");
                SQL.AppendLine("FROM [MappingProject] m ");
                SQL.AppendLine("LEFT JOIN [CompanyProject] cp ON cp.ID = m.CompanyProjectID ");
                SQL.AppendLine("LEFT JOIN [Company] c ON c.[ID] = cp.[CompanyID] ");
                SQL.AppendLine("WHERE cp.[CompanyID] IN ");
                SQL.AppendLine("(SELECT [ID] FROM [Company] where [Name] like 'clough%') ");
                SQL.AppendLine(" AND (LOWER(m.[DetailMarkup]) like '%href%')");

                updateSQL.AppendLine("UPDATE MappingProject SET Detail = @Detail, DetailMarkup = @DetailMarkup, ChangedOn = GETDATE(), ChangedBy = 'Lawstream Admin'");
                updateSQL.AppendLine("WHERE ID = @MappingID");
            }
            
            try
            {
                dbConn.Connection.Open();

                List<SqlCommand> updateCommandList = new List<SqlCommand>();
                using (SqlTransaction sqlTrans = dbConn.Connection.BeginTransaction("DocumentRegisterTransaction"))
                {
                    try
                    {
                        SqlCommand cmd = dbConn.Connection.CreateCommand();
                        cmd.CommandText = SQL.ToString();
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.Transaction = sqlTrans;

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                try
                                {
                                    int totalRecords = dr.GetInt32(dr.GetOrdinal("TotalRecords"));
                                    int mappingID = dr.GetInt32(dr.GetOrdinal("MappingID"));
                                    string detail = dr.GetString(dr.GetOrdinal("Detail"));
                                    string detailMarkup = dr.GetString(dr.GetOrdinal("Markup"));

                                    recCount++;

                                    if (recCount % 5 == 0)
                                    {
                                        if (OnProgressUpdate != null)
                                        {
                                            ProgressUpdateEventArgs temp = new ProgressUpdateEventArgs(recCount, totalRecords);
                                            OnProgressUpdate(temp);
                                        }
                                    }

                                    // Remove any unwanted chars from the detail markup
                                    detailMarkup = detailMarkup.Replace("\n", " ");

                                    foreach (DocumentURL entry in fileReplacements)
                                    {
                                        // Extract every "href" segment in the detail markup
                                        updatedCount += ReplaceItems(entry.ExistingDoc, entry, ref detail, ref detailMarkup);
                                        updatedCount += ReplaceItems(entry.ExistingDoc.Replace("&nbsp;", "").Replace("&nbsp", ""), entry, ref detail, ref detailMarkup);
                                        updatedCount += ReplaceItems(entry.ExistingDoc.Replace("&amp;", "&"), entry, ref detail, ref detailMarkup);
                                        updatedCount += ReplaceItems(entry.ExistingDoc.Replace("&amp;", "and"), entry, ref detail, ref detailMarkup);
                                        updatedCount += ReplaceItems(entry.CorrectDoc, entry, ref detail, ref detailMarkup);
                                    }

                                    // Space out any joinedtogether items
                                    detail = detail.Replace("__", "_ _");
                                    detailMarkup = detailMarkup.Replace("__", "_ _");
                                    
                                    // Collect the update commands
                                    SqlCommand updateCmd = dbConn.Connection.CreateCommand();
                                    updateCmd.CommandText = updateSQL.ToString();
                                    updateCmd.CommandType = System.Data.CommandType.Text;
                                    updateCmd.Transaction = sqlTrans;

                                    SqlParameter detailParam = updateCmd.Parameters.Add("@Detail", System.Data.SqlDbType.NVarChar, -1);
                                    SqlParameter detailMarkupParam = updateCmd.Parameters.Add("@DetailMarkup", System.Data.SqlDbType.NVarChar, -1);
                                    SqlParameter idParam = updateCmd.Parameters.Add("@MappingID", System.Data.SqlDbType.Int);

                                    detailParam.Value = detail;
                                    detailMarkupParam.Value = detailMarkup;
                                    idParam.Value = mappingID;

                                    updateCommandList.Add(updateCmd);
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception(ex.Message);
                                }
                            }
                        }

                        // Now fire off the SQL commands
                        foreach (SqlCommand updateCmd in updateCommandList)
                        {
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        sqlTrans.Rollback();
                        throw new Exception(ex.Message);
                    }

                    sqlTrans.Commit();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                dbConn.Connection.Close();
            }

            return updatedCount;
        }

        private int ReplaceItems(string docName, DocumentURL entry, ref string detail, ref string detailMarkup)
        {
            int updateCount = 0;

            try
            {
                int startIndex = detailMarkup.ToLower().IndexOf(">" + docName.ToLower() + "<");

                // Check for scrwups - spacing etc
                if (startIndex < 0)
                {
                    startIndex = detailMarkup.ToLower().IndexOf(">" + docName.ToLower() + " <");
                }

                if (startIndex < 0)
                {
                    startIndex = detailMarkup.ToLower().IndexOf("> " + docName.ToLower() + "<");
                }

                if (startIndex < 0)
                {
                    startIndex = detailMarkup.ToLower().IndexOf("> " + docName.ToLower() + " <");
                }

                if (startIndex > 0)
                {
                    // Now find the "<a" start posn
                    string prevPartOfMarkup = detailMarkup.Substring(0, startIndex);

                    int linkStart = prevPartOfMarkup.ToLower().LastIndexOf("<a");
                    int endLinkPosn = detailMarkup.ToLower().IndexOf("</a>", startIndex);

                    if ((linkStart > -1) && (endLinkPosn > -1))
                    {
                        if (endLinkPosn > linkStart)
                        {
                            string hrefLink = detailMarkup.Substring(linkStart, ((endLinkPosn + 4) - linkStart)).Trim();

                            // Replace the href with the actual registered document link
                            string replacementHref = "_" + entry.CorrectDoc + "_";
                            detailMarkup = detailMarkup.Replace(hrefLink, replacementHref);

                            // And do so in the detail section too
                            if (!detail.Contains(replacementHref))
                            {
                                detail = Regex.Replace(detail, entry.CorrectDoc, replacementHref, RegexOptions.IgnoreCase);

                                // Do this after as it wll be the shorter string
                                string existingDoc = entry.ExistingDoc.Replace("&nbsp;", "").Replace("&nbsp", "").Replace("&amp;", "&").Replace("&ndash;", "-").Replace(" / ", ", ");
                                detail = Regex.Replace(detail, existingDoc, replacementHref, RegexOptions.IgnoreCase);
                            }

                            updateCount++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return updateCount;
        }

        #endregion

        #region Old

        ///// <summary>
        ///// Updates the URLs in the database from the existing (incorrect) to new (correct) ones. This gives us a 
        ///// uniform place to start replacing the URLs in the database with the registered document markup.
        ///// </summary>
        ///// <param name="dbConn">The database connection.</param>
        ///// <param name="recCount">The record count.</param>
        ///// <param name="updateCount">The update count.</param>
        ///// <exception cref="Exception">
        ///// Couldn't find the all the columns!
        ///// or
        ///// or
        ///// </exception>
        //public void UpdateExistingURLsToCorrect(DBConnection dbConn, out int recCount, out int updateCount)
        //{
        //    List<DocumentURL> fileReplacements = new List<DocumentURL>();

        //    recCount = 0;
        //    updateCount = 0;
        //    int totalCount = 0;
        //    StringBuilder updateSQL = new StringBuilder(1000);

        //    DataSourceTextFile dataSource = new DataSourceTextFile();
        //    string sourceFilePath = this.FolderPath + @"\Clough URL Changes Fixed.csv";

        //    try
        //    {
        //        // Count the lines in the file
        //        using (StreamReader r = new StreamReader(sourceFilePath))
        //        {
        //            int i = 0;
        //            while (r.ReadLine() != null) { totalCount++; }
        //        }

        //        // Use SQL to read and update the file
        //        dataSource.DatabaseType = ConstantValues.ConnectionType.Text_File;
        //        dataSource.FileType = ConstantValues.VersionType.CommaDelim;
        //        dataSource.SourceFilePath = sourceFilePath;

        //        string selectSQL = "SELECT * FROM [" + dataSource.SourceFile + "]";

        //        // Read the file, find the headers we need
        //        using (IDbConnection connSource = dataSource.NewConnection())
        //        {
        //            using (IDataReader dr = dataSource.SelectData(selectSQL, connSource))
        //            {
        //                int docReadCount = 0;

        //                try
        //                {
        //                    while (dr.Read())
        //                    {
        //                        docReadCount++;
        //                        string existingDocument = dr.GetString(dr.GetOrdinal("Document"));

        //                        string existingURL = string.Empty;
        //                        if (!dr.IsDBNull(dr.GetOrdinal("Existing URL")))
        //                        {
        //                            existingURL = dr.GetString(dr.GetOrdinal("Existing URL"));
        //                        }

        //                        string correctDoc = dr.GetString(dr.GetOrdinal("Correct Document Name"));
        //                        string correctURL = dr.GetString(dr.GetOrdinal("Correct URL"));

        //                        DocumentURL docURL = new DocumentURL();
        //                        docURL.ExistingDoc = existingDocument;
        //                        docURL.ExistingURL = existingURL;
        //                        docURL.CorrectDoc = correctDoc;
        //                        docURL.CorrectURL = correctURL;

        //                        DocumentURL test = fileReplacements.Find(x => x.ExistingDoc == docURL.ExistingDoc);

        //                        if (test == null)
        //                        {
        //                            fileReplacements.Add(docURL);
        //                        }
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    throw new Exception(ex.Message);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }

        //    // Now go and do the actual replacements
        //    StringBuilder SQL = new StringBuilder();
        //    SQL.AppendLine("UPDATE Mapping SET Detail = REPLACE(Detail, @ExistingDoc, @CorrectDoc), DetailMarkup = REPLACE(DetailMarkup, @ExistingHref, @ReplacementHref), ChangedOn = GETDATE(), ChangedBy = 'Lawstream Admin'");
        //    SQL.AppendLine("WHERE ID = @MappingID;");

        //    SQL.AppendLine("UPDATE Mapping SET Detail = REPLACE(Detail, @ExistingDoc2, @CorrectDoc)");
        //    SQL.AppendLine("WHERE ID = @MappingID;");

        //    SQL.AppendLine("UPDATE Mapping SET Detail = REPLACE(Detail, @ExistingDoc3, @CorrectDoc)");
        //    SQL.AppendLine("WHERE ID = @MappingID");


        //    StringBuilder projectSQL = new StringBuilder();
        //    projectSQL.AppendLine("UPDATE MappingProject SET Detail = REPLACE(Detail, @ExistingDoc, @CorrectDoc), DetailMarkup = REPLACE(DetailMarkup, @ExistingHref, @ReplacementHref), ChangedOn = GETDATE(), ChangedBy = 'Lawstream Admin'");
        //    projectSQL.AppendLine("WHERE ID = @MappingID");

        //    projectSQL.AppendLine("UPDATE MappingProject SET Detail = REPLACE(Detail, @ExistingDoc2, @CorrectDoc)");
        //    projectSQL.AppendLine("WHERE ID = @MappingID;");

        //    projectSQL.AppendLine("UPDATE MappingProject SET Detail = REPLACE(Detail, @ExistingDoc3, @CorrectDoc)");
        //    projectSQL.AppendLine("WHERE ID = @MappingID");

        //    try
        //    {
        //        dbConn.Connection.Open();

        //        using (SqlTransaction sqlTrans = dbConn.Connection.BeginTransaction("SampleTransaction"))
        //        {
        //            try
        //            {

        //                // Build up the existing and replacement HTML.
        //                SqlCommand cmd = dbConn.Connection.CreateCommand();
        //                cmd.CommandText = SQL.ToString();
        //                cmd.CommandType = System.Data.CommandType.Text;
        //                cmd.Transaction = sqlTrans;

        //                SqlParameter existingHrefParam = cmd.Parameters.Add("@ExistingHref", System.Data.SqlDbType.NVarChar, -1);
        //                SqlParameter replacementHrefParam = cmd.Parameters.Add("@ReplacementHref", System.Data.SqlDbType.NVarChar, -1);
        //                SqlParameter existingDocParam = cmd.Parameters.Add("@ExistingDoc", System.Data.SqlDbType.NVarChar, -1);
        //                SqlParameter existingDocParam2 = cmd.Parameters.Add("@ExistingDoc2", System.Data.SqlDbType.NVarChar, -1);
        //                SqlParameter existingDocParam3 = cmd.Parameters.Add("@ExistingDoc3", System.Data.SqlDbType.NVarChar, -1);
        //                SqlParameter replacementDocParam = cmd.Parameters.Add("@CorrectDoc", System.Data.SqlDbType.NVarChar, -1);
        //                SqlParameter idParam = cmd.Parameters.Add("@MappingID", System.Data.SqlDbType.Int);

        //                // Build up the existing and replacement HTML.
        //                foreach (DocumentURL entry in fileReplacements)
        //                {
        //                    recCount++;

        //                    if (recCount % 5 == 0)
        //                    {
        //                        if (OnProgressUpdate != null)
        //                        {
        //                            ProgressUpdateEventArgs temp = new ProgressUpdateEventArgs(recCount, fileReplacements.Count);
        //                            OnProgressUpdate(temp);
        //                        }
        //                    }

        //                    // Build up the existing <a href...>Doc</a> string and it's replacement. Then replace via t-sql.

        //                    // Normal mappings first, then project mappings
        //                    Dictionary<int, string> existHrefList = GetDocumentAndLink(dbConn, sqlTrans, entry.ExistingDoc, false);

        //                    string replacementHref = "<a target=\"blank\" href=\"" + entry.CorrectURL + ">" + entry.CorrectDoc + "</a>";

        //                    replacementHrefParam.Value = replacementHref;

        //                    existingDocParam.Value = entry.ExistingDoc.Replace("&nbsp", "").Replace("&nbsp;", "");
        //                    replacementDocParam.Value = entry.CorrectDoc;

        //                    // And the potential variations on the existing doc name
        //                    int corpIndex = entry.CorrectDoc.IndexOf("CORP");
        //                    if (corpIndex > 0)
        //                    {
        //                        string variant2 = entry.CorrectDoc.Substring(0, corpIndex).Trim();
        //                        existingDocParam2.Value = variant2;
        //                    }
        //                    else
        //                    {
        //                        existingDocParam2.Value = null;
        //                    }

        //                    existingDocParam3.Value = entry.CorrectDoc.Replace("&nbsp", "").Replace("&nbsp;", "");

        //                    foreach (KeyValuePair<int, string> hrefLink in existHrefList)
        //                    {

        //                        if (replacementHref != hrefLink.Value)
        //                        {
        //                            idParam.Value = hrefLink.Key;
        //                            existingHrefParam.Value = hrefLink.Value;

        //                            int updated = cmd.ExecuteNonQuery();
        //                            updateCount += updated;
        //                        }
        //                    }

        //                    // Now projects
        //                    SqlCommand projectCmd = dbConn.Connection.CreateCommand();
        //                    projectCmd.CommandText = projectSQL.ToString();
        //                    projectCmd.CommandType = System.Data.CommandType.Text;
        //                    projectCmd.Transaction = sqlTrans;

        //                    SqlParameter existingHrefProjectParam = projectCmd.Parameters.Add("@ExistingHref", System.Data.SqlDbType.NVarChar, -1);
        //                    SqlParameter replacementHrefProjectParam = projectCmd.Parameters.Add("@ReplacementHref", System.Data.SqlDbType.NVarChar, -1);
        //                    SqlParameter existingDocProjectParam = projectCmd.Parameters.Add("@ExistingDoc", System.Data.SqlDbType.NVarChar, -1);
        //                    SqlParameter existingDocProjectParam2 = projectCmd.Parameters.Add("@ExistingDoc2", System.Data.SqlDbType.NVarChar, -1);
        //                    SqlParameter existingDocProjectParam3 = projectCmd.Parameters.Add("@ExistingDoc3", System.Data.SqlDbType.NVarChar, -1);
        //                    SqlParameter replacementDocProjectParam = projectCmd.Parameters.Add("@CorrectDoc", System.Data.SqlDbType.NVarChar, -1);
        //                    SqlParameter idProjectParam = projectCmd.Parameters.Add("@MappingID", System.Data.SqlDbType.Int);

        //                    existHrefList = GetDocumentAndLink(dbConn, sqlTrans, entry.ExistingDoc, true);

        //                    replacementHrefProjectParam.Value = replacementHref;

        //                    existingDocProjectParam.Value = entry.ExistingDoc.Replace("&nbsp", "").Replace("&nbsp;", "");
        //                    replacementDocProjectParam.Value = entry.CorrectDoc;

        //                    // Variants on the doc name
        //                    if (corpIndex > 0)
        //                    {
        //                        string variant2 = entry.CorrectDoc.Substring(0, corpIndex).Trim();
        //                        existingDocProjectParam2.Value = variant2;
        //                        existingDocProjectParam3.Value = entry.CorrectDoc.Replace("&nbsp", "").Replace("&nbsp;", "");
        //                    }
        //                    else
        //                    {
        //                        existingDocProjectParam2.Value = null;
        //                        existingDocProjectParam3.Value = null;
        //                    }

        //                    foreach (KeyValuePair<int, string> hrefLink in existHrefList)
        //                    {

        //                        if (replacementHref != hrefLink.Value)
        //                        {
        //                            idProjectParam.Value = hrefLink.Key;
        //                            existingHrefProjectParam.Value = hrefLink.Value;

        //                            int updated = projectCmd.ExecuteNonQuery();
        //                            updateCount += updated;
        //                        }
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                sqlTrans.Rollback();
        //                throw new Exception(ex.Message);
        //            }

        //            sqlTrans.Commit();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //    finally
        //    {
        //        dbConn.Connection.Close();
        //    }
        //}

        //private int UpdateRegisteredDocMarkupInCode(DBConnection dbConn, List<DocumentURL> fileReplacements, bool isProject)
        //{
        //    int updatedCount = 0;
        //    int recCount = 0;

        //    // Now go and do the actual replacements
        //    StringBuilder SQL = new StringBuilder();
        //    StringBuilder updateSQL = new StringBuilder();

        //    if (!isProject)
        //    {
        //        SQL.AppendLine("SELECT m.[ID] AS [MappingID], m.[Detail] AS [Detail], m.[DetailMarkup] AS [Markup], ");
        //        SQL.AppendLine("ld.[ID] AS [LegislationID], ld.[Name] AS [Legislation], lm.[ID] AS [Legislation Subsection ID], lm.[Name] AS [Legislation Subsection], ");
        //        SQL.AppendLine("c.[ID] AS [CompanyID], c.[Name] AS [Company], NULL AS [ProjectID], NULL AS [Project] ");
        //        SQL.AppendLine("FROM [Mapping] m ");
        //        SQL.AppendLine("LEFT JOIN Legislation l ON l.ID = m.LegislationID ");
        //        SQL.AppendLine("LEFT JOIN LegislationDefinition ld ON ld.ID = l.LegislationDefinitionID  ");
        //        SQL.AppendLine("LEFT JOIN LegislationMetadata lm ON lm.ID = l.LegislationMetadataID ");
        //        SQL.AppendLine("LEFT JOIN[User] u ON u.ID = m.UserID ");
        //        SQL.AppendLine("LEFT JOIN [Company] c ON c.[ID] = u.[CompanyID] ");
        //        SQL.AppendLine("WHERE u.CompanyID IN ");
        //        SQL.AppendLine("    (SELECT [ID] FROM [Company] where [Name] like 'clough%') ");
        //        SQL.AppendLine("AND m.[ID] = 20926");

        //        updateSQL.AppendLine("UPDATE Mapping SET Detail = @Detail, DetailMarkup = @DetailMarkup");
        //        updateSQL.AppendLine("WHERE ID = @MappingID");            }
        //    else
        //    {
        //        SQL.AppendLine("SELECT m.[ID] AS [MappingID], m.[Detail] AS [Detail], m.[DetailMarkup] AS [Markup], ");
        //        SQL.AppendLine("ld.[ID] AS [LegislationID], ld.[Name] AS [Legislation], lm.[ID] AS [Legislation Subsection ID], lm.[Name] AS [Legislation Subsection], ");
        //        SQL.AppendLine("c.[ID] AS [CompanyID], c.[Name] AS [Company], cp.[ID] AS [ProjectID], cp.[Name] AS [Project] ");
        //        SQL.AppendLine("FROM [MappingProject] m ");
        //        SQL.AppendLine("LEFT JOIN Legislation l ON l.ID = m.LegislationID ");
        //        SQL.AppendLine("LEFT JOIN LegislationDefinition ld ON ld.ID = l.LegislationDefinitionID  ");
        //        SQL.AppendLine("LEFT JOIN LegislationMetadata lm ON lm.ID = l.LegislationMetadataID ");
        //        SQL.AppendLine("LEFT JOIN [CompanyProject] cp ON cp.ID = m.CompanyProjectID ");
        //        SQL.AppendLine("LEFT JOIN [Company] c ON c.[ID] = cp.[CompanyID] ");
        //        SQL.AppendLine("WHERE cp.[CompanyID] IN ");
        //        SQL.AppendLine("(SELECT [ID] FROM [Company] where [Name] like 'clough%') ");

        //        updateSQL.AppendLine("UPDATE MappingProject SET Detail = @Detail, DetailMarkup = @DetailMarkup");
        //        updateSQL.AppendLine("WHERE ID = @MappingID");

        //    }

        //    try
        //    {
        //        dbConn.Connection.Open();

        //        using (SqlTransaction sqlTrans = dbConn.Connection.BeginTransaction("DocumentRegisterTransaction"))
        //        {
        //            try
        //            {
        //                SqlCommand cmd = dbConn.Connection.CreateCommand();
        //                cmd.CommandText = SQL.ToString();
        //                cmd.CommandType = System.Data.CommandType.Text;
        //                cmd.Transaction = sqlTrans;

        //                SqlCommand updateCmd = dbConn.Connection.CreateCommand();
        //                updateCmd.CommandText = updateSQL.ToString();
        //                updateCmd.CommandType = System.Data.CommandType.Text;
        //                updateCmd.Transaction = sqlTrans;

        //                SqlParameter detailParam = updateCmd.Parameters.Add("@Detail", System.Data.SqlDbType.NVarChar, -1);
        //                SqlParameter detailMarkupParam = updateCmd.Parameters.Add("@DetailMarkup", System.Data.SqlDbType.NVarChar, -1);
        //                SqlParameter idParam = updateCmd.Parameters.Add("@MappingID", System.Data.SqlDbType.Int);

        //                using (SqlDataReader dr = cmd.ExecuteReader())
        //                {
        //                    while (dr.Read())
        //                    {

        //                        int mappingID = dr.GetInt32(dr.GetOrdinal("MappingID"));
        //                        string detail = dr.GetString(dr.GetOrdinal("Detail"));
        //                        string detailMarkup = dr.GetString(dr.GetOrdinal("Markup"));

        //                        foreach (DocumentURL entry in fileReplacements)
        //                        {
        //                            recCount++;

        //                            if (recCount % 5 == 0)
        //                            {
        //                                if (OnProgressUpdate != null)
        //                                {
        //                                    ProgressUpdateEventArgs temp = new ProgressUpdateEventArgs(recCount, fileReplacements.Count);
        //                                    OnProgressUpdate(temp);
        //                                }
        //                            }

        //                            // Extract every "href" segment in the detail markup
        //                            int startIndex = detailMarkup.IndexOf(">" + entry.CorrectDoc + "<");

        //                            if (startIndex > 0)
        //                            {
        //                                // Now find the "<a" start posn
        //                                string prevPartOfMarkup = detailMarkup.Substring(0, startIndex);

        //                                int linkStart = prevPartOfMarkup.LastIndexOf("<a");
        //                                int endLinkPosn = detailMarkup.IndexOf("</a>", startIndex);

        //                                if ((linkStart > -1) && (endLinkPosn > -1))
        //                                {
        //                                    if (endLinkPosn > linkStart)
        //                                    {
        //                                        string hrefLink = detailMarkup.Substring(linkStart, ((endLinkPosn + 4) - linkStart)).Trim();

        //                                        // Replace the href with the actual registered document link
        //                                        string replacementHref = "_" + entry.CorrectDoc + "_";
        //                                        detailMarkup = detailMarkup.Replace(hrefLink, replacementHref);

        //                                        // And do so in the detail section too
        //                                        detail = detail.Replace(entry.CorrectDoc, replacementHref);

        //                                        detailParam.Value = detail;
        //                                        detailMarkupParam.Value = detailMarkup;
        //                                        idParam.Value = mappingID;

        //                                        int updated = updateCmd.ExecuteNonQuery();
        //                                        updatedCount += updated;
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                sqlTrans.Rollback();
        //                throw new Exception(ex.Message);
        //            }

        //            sqlTrans.Commit();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //    finally
        //    {
        //        dbConn.Connection.Close();
        //    }

        //    return updatedCount;
        //}

        ///// <summary>
        ///// Updates the URLs in the database with the registered document markup.
        ///// </summary>
        ///// <param name="dbConn">The database connection.</param>
        ///// <param name="recCount">The record count.</param>
        ///// <param name="updateCount">The update count.</param>
        ///// <exception cref="Exception">
        ///// Couldn't find the all the columns!
        ///// or
        ///// or
        ///// </exception>
        //public void UpdateExistingURLsToRegisteredDocumentMarkup(DBConnection dbConn, out int recCount, out int updateCount)
        //{
        //    List<DocumentURL> fileReplacements = new List<DocumentURL>();

        //    recCount = 0;
        //    updateCount = 0;
        //    int totalCount = 0;
        //    StringBuilder updateSQL = new StringBuilder(1000);

        //    DataSourceTextFile dataSource = new DataSourceTextFile();
        //    string sourceFilePath = this.FolderPath + @"\Clough URL Changes Fixed.csv";

        //    try
        //    {
        //        // Count the lines in the file
        //        using (StreamReader r = new StreamReader(sourceFilePath))
        //        {
        //            int i = 0;
        //            while (r.ReadLine() != null) { totalCount++; }
        //        }

        //        // Use SQL to read and update the file
        //        dataSource.DatabaseType = ConstantValues.ConnectionType.Text_File;
        //        dataSource.FileType = ConstantValues.VersionType.CommaDelim;
        //        dataSource.SourceFilePath = sourceFilePath;

        //        string selectSQL = "SELECT * FROM [" + dataSource.SourceFile + "]";

        //        // Read the file, find the headers we need
        //        using (IDbConnection connSource = dataSource.NewConnection())
        //        {
        //            using (IDataReader dr = dataSource.SelectData(selectSQL, connSource))
        //            {
        //                try
        //                {
        //                    while (dr.Read())
        //                    {
        //                        string existingDocument = dr.GetString(dr.GetOrdinal("Document"));

        //                        string existingURL = string.Empty;
        //                        if (!dr.IsDBNull(dr.GetOrdinal("Existing URL")))
        //                        {
        //                            existingURL = dr.GetString(dr.GetOrdinal("Existing URL"));
        //                        }

        //                        string correctDoc = dr.GetString(dr.GetOrdinal("Correct Document Name"));
        //                        string correctURL = dr.GetString(dr.GetOrdinal("Correct URL"));

        //                        DocumentURL docURL = new DocumentURL();
        //                        docURL.ExistingDoc = existingDocument;
        //                        docURL.ExistingURL = existingURL;
        //                        docURL.CorrectDoc = correctDoc;
        //                        docURL.CorrectURL = correctURL;

        //                        DocumentURL test = fileReplacements.Find(x => x.ExistingDoc == docURL.ExistingDoc);

        //                        if (test == null)
        //                        {
        //                            fileReplacements.Add(docURL);
        //                        }
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    throw new Exception(ex.Message);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }

        //    // Now go and do the actual replacements
        //    StringBuilder SQL = new StringBuilder();
        //    SQL.AppendLine("UPDATE Mapping SET Detail = REPLACE(Detail, @CorrectDoc, @ReplacementDocumentMarkup), ");
        //    SQL.AppendLine("DetailMarkup = REPLACE(DetailMarkup, @ExistingHref, @ReplacementDocumentMarkup), ChangedOn = GETDATE(), ChangedBy = 'Lawstream Admin'");
        //    SQL.AppendLine("WHERE ID = @MappingID;");

        //    StringBuilder projectSQL = new StringBuilder();
        //    projectSQL.AppendLine("UPDATE MappingProject SET Detail = REPLACE(Detail, @CorrectDoc, @ReplacementDocumentMarkup), ");
        //    projectSQL.AppendLine("DetailMarkup = REPLACE(DetailMarkup, @ExistingHref, @ReplacementDocumentMarkup), ChangedOn = GETDATE(), ChangedBy = 'Lawstream Admin'");
        //    projectSQL.AppendLine("WHERE ID = @MappingID;");

        //    try
        //    {
        //        dbConn.Connection.Open();

        //        using (SqlTransaction sqlTrans = dbConn.Connection.BeginTransaction("DocumentRegisterTransaction"))
        //        {
        //            try
        //            {
        //                // Build up the existing and replacement HTML.
        //                SqlCommand cmd = dbConn.Connection.CreateCommand();
        //                cmd.CommandText = SQL.ToString();
        //                cmd.CommandType = System.Data.CommandType.Text;
        //                cmd.Transaction = sqlTrans;

        //                SqlParameter existingHrefParam = cmd.Parameters.Add("@ExistingHref", System.Data.SqlDbType.NVarChar, -1);
        //                SqlParameter replacementHrefParam = cmd.Parameters.Add("@ReplacementDocumentMarkup", System.Data.SqlDbType.NVarChar, -1);
        //                SqlParameter existingDocParam = cmd.Parameters.Add("@CorrectDoc", System.Data.SqlDbType.NVarChar, -1);
        //                SqlParameter idParam = cmd.Parameters.Add("@MappingID", System.Data.SqlDbType.Int);

        //                foreach (DocumentURL entry in fileReplacements)
        //                {
        //                    recCount++;

        //                    if (recCount % 5 == 0)
        //                    {
        //                        if (OnProgressUpdate != null)
        //                        {
        //                            ProgressUpdateEventArgs temp = new ProgressUpdateEventArgs(recCount, fileReplacements.Count);
        //                            OnProgressUpdate(temp);
        //                        }
        //                    }

        //                    #region Standard Mappings

        //                    // Build up the existing <a href...>Doc</a> string and it's replacement. Then replace via t-sql.
        //                    Dictionary<int, string> existHrefList = GetDocumentAndLink(dbConn, sqlTrans, entry.CorrectDoc, false);

        //                    string replacementHref = "_" + entry.CorrectDoc + "_";

        //                    replacementHrefParam.Value = replacementHref;
        //                    existingDocParam.Value = entry.CorrectDoc.Replace("&nbsp", "").Replace("&nbsp;", "");

        //                    foreach (KeyValuePair<int, string> hrefLink in existHrefList)
        //                    {
        //                        if (replacementHref != hrefLink.Value)
        //                        {
        //                            idParam.Value = hrefLink.Key;
        //                            existingHrefParam.Value = hrefLink.Value;

        //                            int updated = cmd.ExecuteNonQuery();
        //                            updateCount += updated;
        //                        }
        //                    }

        //                    #endregion

        //                    #region Project Mappings

        //                    // Build up the existing and replacement HTML.
        //                    SqlCommand projectCmd = dbConn.Connection.CreateCommand();
        //                    projectCmd.CommandText = projectSQL.ToString();
        //                    projectCmd.CommandType = System.Data.CommandType.Text;
        //                    projectCmd.Transaction = sqlTrans;

        //                    SqlParameter existingHrefProjectParam = projectCmd.Parameters.Add("@ExistingHref", System.Data.SqlDbType.NVarChar, -1);
        //                    SqlParameter replacementHrefProjectParam = projectCmd.Parameters.Add("@ReplacementDocumentMarkup", System.Data.SqlDbType.NVarChar, -1);
        //                    SqlParameter existingDocProjectParam = projectCmd.Parameters.Add("@CorrectDoc", System.Data.SqlDbType.NVarChar, -1);
        //                    SqlParameter idProjectParam = projectCmd.Parameters.Add("@MappingID", System.Data.SqlDbType.Int);

        //                    // Build up the existing <a href...>Doc</a> string and it's replacement. Then replace via t-sql.
        //                    existHrefList = GetDocumentAndLink(dbConn, sqlTrans, entry.CorrectDoc, true);

        //                    replacementHrefProjectParam.Value = replacementHref;
        //                    existingDocProjectParam.Value = entry.CorrectDoc.Replace("&nbsp", "").Replace("&nbsp;", "");

        //                    foreach (KeyValuePair<int, string> hrefLink in existHrefList)
        //                    {
        //                        if (replacementHref != hrefLink.Value)
        //                        {
        //                            idProjectParam.Value = hrefLink.Key;
        //                            existingHrefProjectParam.Value = hrefLink.Value;

        //                            int updated = projectCmd.ExecuteNonQuery();
        //                            updateCount += updated;
        //                        }
        //                    }

        //                    #endregion

        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                sqlTrans.Rollback();
        //                throw new Exception(ex.Message);
        //            }

        //            sqlTrans.Commit();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //    finally
        //    {
        //        dbConn.Connection.Close();
        //    }
        //}

        #endregion

        #region Classes 

        public class DocumentData
        {
            public DocumentData()
            {
                this.DocumentList = new List<KeyValuePair<string, string>>();
            }

            public int MappingID
            {
                get;
                set;
            }

            public string Markup
            {
                get;
                set;
            }

            public int LegislationID
            {
                get;
                set;
            }

            public string Legislation
            {
                get;
                set;
            }

            public int LegislationSubsectionID
            {
                get;
                set;
            }

            public string LegislationSubsection
            {
                get;
                set;
            }

            public int CompanyID
            {
                get;
                set;
            }

            public string Company
            {
                get;
                set;
            }

            public Guid? ProjectID
            {
                get;
                set;
            }

            public string Project
            {
                get;
                set;
            }

            public List<KeyValuePair<string, string>> DocumentList
            {
                get;
                set;
            }
        }

        public class DocumentURL
        {
            public DocumentURL()
            {
            }

            public string Legislation
            {
                get;
                set;
            }

            public string LegislationMetadata
            {
                get;
                set;
            }

            public string Project
            {
                get;
                set;
            }

            public string ExistingDoc
            {
                get;
                set;
            }

            public string ExistingURL
            {
                get;
                set;
            }

            public string CorrectDoc
            {
                get;
                set;
            }

            public string CorrectURL
            {
                get;
                set;
            }
        }

        #endregion
    }
}
