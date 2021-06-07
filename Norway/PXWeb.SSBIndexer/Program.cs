using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using PCAxis.Paxiom.Extensions;
using PCAxis.Search;

using PCAxis.Menu;
using PCAxis.Menu.Implementations;
using PCAxis.Web.Controls;

namespace PXWeb.SSBIndexer
{
    /// <summary>
    /// Program for creating/updating search index for a CNMM database
    /// </summary>
    class Program
    {
        private static log4net.ILog _logger;
        private static string _task;
        private static string _dbDir;
        private static List<string> _languages;
        private static string _database;
        private static string _configPath;
        private static string _startStatus;
        private static DateTime _lastUpdate;
        private static string _updateMethod;
        private static GetMenuDelegate _menuMethod = new PCAxis.Search.GetMenuDelegate(GetMenuAndItem);

        static void Main(string[] args)
        {
#if DEBUG
            
            //args = new[] { "update", @"C:\pc-axis\Customized\SSB\PX-Web_2016_V1\PXWeb\Resources\PX\Databases\", "table", "no" };
           // args = new[] { "create", @"C:\pc-axis\Customized\SSB\PX-Web_2016_V1\PXWeb\Resources\PX\Databases\", "utv", "no" };
#endif
            _logger = log4net.LogManager.GetLogger(typeof(Program));
            _logger.InfoFormat("=== SSBIndexer started ===");

            if (!GetParameters(args))
                return;

            if (!VerifyConfigFile())
                return;

            if (!PrepareTask())
                return;

            bool success = false;

            if (_task == "create")
            {
                success = CreateIndex();
            }
            else if (_task == "update")
            {
                success = UpdateIndex();
            }

            if (!EndTask(success))
                return;

            if (success)
            {
                _logger.InfoFormat("=== SSBIndexer avslutta korrekt ===");
            }
            else
            {
                _logger.InfoFormat("=== SSBIndexer avslutta med FEIL!!!!! ===");
            }
        }

        /// <summary>
        /// Create search index
        /// </summary>
        private static bool CreateIndex()
        {
            _logger.InfoFormat("Create Index started");

            try
            {
                foreach (string language in _languages)
                {
                    _logger.InfoFormat("Creating Index for {0} - {1}", _database, language);

                    Indexer indexer = new Indexer(GetIndexDirectoryPath(_database, language), _menuMethod, _database, language);

                    if (indexer.CreateIndex())
                    {
                        _logger.InfoFormat("Successfully created Index for {0} - {1}", _database, language);
                    }
                    else
                    {
                        _logger.ErrorFormat("Failed to create Index for {0} - {1}", _database, language);
                        return false;
                    }
                }

                _logger.InfoFormat("Create Index ended successfully");
                return true;
            }
            catch(Exception e)
            {
                _logger.ErrorFormat("Failed to create Index for {0}", _database);
                _logger.ErrorFormat(e.InnerException.ToString());
                return false;
            }

        }

        /// <summary>
        /// Update search index
        /// </summary>
        private static bool UpdateIndex()
        {
            _logger.InfoFormat("Update Index started");
            try
            {
                ISearchIndex updater = GetUpdater();

                if (updater == null)
                    return false;

                foreach (string language in _languages)
                {
                    List<PCAxis.Search.TableUpdate> lst = updater.GetUpdatedTables(_lastUpdate, _database, language);

                    _logger.InfoFormat("{0} tables are updated in {1} - {2}", lst.Count, _database, language);

                    if (lst.Count > 0)
                    {
                        Indexer indexer = new Indexer(GetIndexDirectoryPath(_database, language), _menuMethod, _database, language);

                        if (indexer.UpdateIndex(lst))
                        {
                            _logger.Info("Successfully updated the " + _database + " - " + language + " search index");
                        }
                        else
                        {
                            _logger.Error("Failed to update the " + _database + " - " + language + " search index");
                            return false;
                        }
                    }
                    else
                    {
                        _logger.Info("Skipping update of search index...");
                    }
                }
                _logger.InfoFormat("Update Index ended");
                return true;
            }
            catch(Exception e)
            {
                _logger.Error("Failed to update the " + _database + " - " + "search index");
                _logger.ErrorFormat(e.InnerException.ToString());
                return false;
            }
        }

        /// <summary>
        /// Get path to the specified index directory 
        /// </summary>
        /// <param name="database">database</param>
        /// <param name="language">language</param>
        /// <returns></returns>
        private static string GetIndexDirectoryPath(string database, string language)
        {
            return Path.Combine(_dbDir, database, "_INDEX", language);
        }

        /// <summary>
        /// Get object for updating the search index
        /// </summary>
        /// <returns></returns>
        private static ISearchIndex GetUpdater()
        {
            ISearchIndex updater;

            try
            {
                if (string.IsNullOrEmpty(_updateMethod))
                {
                    updater = new DefaultSearchIndex();
                }
                else
                {
                    var typeString = _updateMethod;
                    var parts = typeString.Split(',');
                    var typeName = parts[0].Trim();
                    var assemblyName = parts[1].Trim();
                    updater = (ISearchIndex)Activator.CreateInstance(assemblyName, typeName).Unwrap();
                }
            }
            catch (Exception)
            {
                _logger.Error("Could not create index updater object: '" + _updateMethod + "'");
                return null;
            }

            return updater;
        }

        /// <summary>
        /// Opens the database.config file and verifies that no one else is indexing the search index right now.
        /// If not, search index status is set to "Indexing" in the file.
        /// </summary>
        /// <returns>True if preparations could be done, else false</returns>
        private static bool PrepareTask()
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(_configPath);
            XmlNode node = xdoc.SelectSingleNode("/settings/searchIndex/status");

            if (node == null)
            {
                _logger.Error("PrepareTask: Could not get search index status");
                return false;
            }

            if (node.InnerText != "Indexing")
            {
                _startStatus = node.InnerText; // Keep if we need to restore if something goes wrong...
                node.InnerText = "Indexing";

                node = xdoc.SelectSingleNode("/settings/searchIndex/indexUpdated");
                
                if (node == null)
                {
                    _logger.Error("PrepareTask: Could not get search index updated");
                    return false;
                }

                if (node.InnerText.IsPxDate())
                {
                    _lastUpdate = node.InnerText.PxDateStringToDateTime();
                }

                node = xdoc.SelectSingleNode("/settings/searchIndex/updateMethod");

                if (node == null)
                {
                    _logger.Error("PrepareTask: Could not get search index update method");
                    return false;
                }

                _updateMethod = node.InnerText;

                xdoc.Save(_configPath);
                return true;
            }

            _logger.Error("Task aborted - Search index status is 'Indexing'");
            return false;
        }

        /// <summary>
        /// Set search index status after completed task
        /// </summary>
        /// <param name="success">If task was successfull or not</param>
        /// <returns>True if EndTask was successfull, else false</returns>
        private static bool EndTask(bool success)
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(_configPath);

            XmlNode node = xdoc.SelectSingleNode("/settings/searchIndex/status");

            if (node == null)
            {
                _logger.Error("EndTask: Could not get search index status");
                return false;
            }

            if (success)
            {
                node.InnerText = "Indexed";
            }
            else
            {
                //Restore
                //node.InnerText = _startStatus;
                node.InnerText = "Failed";
            }

            node = xdoc.SelectSingleNode("/settings/searchIndex/indexUpdated");

            if (node == null)
            {
                _logger.Error("EndTask: Could not get search index updated");
                return false;
            }

            if (success)
            {
                node.InnerText = DateTime.Now.DateTimeToPxDateString();
            }
            else
            {
                //Restore
                node.InnerText = _lastUpdate.DateTimeToPxDateString();
            }

            xdoc.Save(_configPath);
            return true;
        }       

        /// <summary>
        /// Verifies that the database.config file exists in the database directory
        /// </summary>
        /// <returns></returns>
        private static bool VerifyConfigFile()
        {
            if (File.Exists(Path.Combine(_dbDir, _database, "database.config")))
            {
                _configPath = Path.Combine(_dbDir, _database, "database.config");
                return true;
            }
            else
            {
                _logger.Error("Non-existent database.config file");
                return false;
            }
        }

        /// <summary>
        /// Verifies that valid parameters are passed to the program.
        /// If the parameters are valid they are read into to program.
        /// </summary>
        /// <param name="args">Parameter array</param>
        /// <returns>True if parameters are valid and read into program, else false</returns>
        private static bool GetParameters(string[] args)
        {
            // Parameters:
            // 0 = Task (create or update)
            // 1 = Databases folder
            // 2 = Database
            // 3 = Comma separated list of languages

            _logger.Info("0 task=" + args[0] + " 1 database folder=" + args[1] + "2 database=" + args[2] + "3 lang= " + args[3].Split(new char[] { ',' }).ToString());
            if (args.Length != 4)
            {
                _logger.Error("Parameter error - must be 4 parameters");
                return false;
            }

            if ((args[0].ToLower() != "create") && (args[0].ToLower() != "update"))
            {
                _logger.Error("Parameter error - Illegal action");
                return false;
            }

            if (!Directory.Exists(args[1]))
            {
                _logger.Error("Parameter error - Non-existent directory");
                return false;
            }

            string[] languages = args[3].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (languages.Length == 0)
            {
                _logger.Error("Parameter error - No language specified");
                return false;
            }

            _task = args[0];
            _dbDir = args[1];
            _database = args[2];
            _languages = languages.ToList();

            return true;
        }


        /// <summary>
        /// Get Menu + current item
        /// </summary>
        /// <param name="dbid">Database id</param>
        /// <param name="node">Database node</param>
        /// <param name="lang">Language</param>
        /// <param name="currentItem">Current item (out parameter)</param>
        /// <returns></returns>
        public static PxMenuBase GetMenuAndItem(string dbid, ItemSelection node, string lang, out PCAxis.Menu.Item currentItem)
        {
            //DatabaseInfo dbi = PXWeb.Settings.Current.General.Databases.GetDatabase(dbid);
            PCAxis.Menu.Item menuItem = null;
            PxMenuBase menu = null;

            try
            {
                string nodeId = PathHandlerFactory.Create(PCAxis.Web.Core.Enums.DatabaseType.CNMM).GetPathString(node);
                 menu = GetCnmmMenuAndItem(dbid, nodeId, lang, out menuItem);
                currentItem = menuItem;
                return menu;
            }
            catch (Exception e)
            {
                _logger.Error("Failed in GetMenuAndItem dbid=" + dbid + " ,nodeId=" + node + ", Error=" + e.Message + ". Creation of search index aborted.");
                currentItem = null;
                return null;
            }
        }

        private static PxMenuBase GetCnmmMenuAndItem(string dbid, string nodeId, string lang, out PCAxis.Menu.Item currentItem)
        {
            //string dbLang = PxContext.GetCnmmDbLanguage(dbid, lang);
            string dbLang = lang; // TODO: Check that database has language...
            TableLink tblFix = null;
            try
            {
                PCAxis.Sql.DbConfig.SqlDbConfig sqlDbConfig = PCAxis.Sql.DbConfig.SqlDbConfigsStatic.DataBases[dbid];   //test piv
            }
            catch (Exception e)
            {
                _logger.Error("Failed in sqlDbConfig  dbid=" + dbid + " ,nodeId=" + nodeId + ", Error=" + e.Message +  ". Creation of search index aborted.");
                currentItem = null;
                return null;
            }
            //Create database object to return
            try
            {
                DatamodelMenu menu = ConfigDatamodelMenu.Create(
                            dbLang,
                            PCAxis.Sql.DbConfig.SqlDbConfigsStatic.DataBases[dbid],
                            m =>
                            {
                                m.RootSelection = string.IsNullOrEmpty(nodeId) ? new ItemSelection() : PathHandlerFactory.Create(PCAxis.Web.Core.Enums.DatabaseType.CNMM).GetSelection(nodeId);
                                m.AlterItemBeforeStorage = item =>
                                {
                                    if (item is PCAxis.Menu.Url)
                                    {
                                        PCAxis.Menu.Url url = (PCAxis.Menu.Url)item;
                                    }
                                    if (item is TableLink)
                                    {
                                        TableLink tbl = (TableLink)item;
                                        string tblId = tbl.ID.Selection;
                                        if (!string.IsNullOrEmpty(dbid))
                                        {
                                            tbl.ID = new ItemSelection(item.ID.Menu, dbid + ":" + tbl.ID.Selection); // Hantering av flera databaser!
                                        }

                                        CustomizeTableTitle(tbl, false);

                                        if (tbl.Published.HasValue)
                                        {
                                            // Store date in the PC-Axis date format
                                            tbl.SetAttribute("modified", tbl.Published.Value.ToString(PCAxis.Paxiom.PXConstant.PXDATEFORMAT));
                                        }
                                        if (string.Compare(tblId + item.ID.Menu, PathHandlerFactory.Create(PCAxis.Web.Core.Enums.DatabaseType.CNMM).GetSelection(nodeId).Selection + PathHandlerFactory.Create(PCAxis.Web.Core.Enums.DatabaseType.CNMM).GetSelection(nodeId).Menu, true) == 0)
                                        //if (string.Compare(tblId, PathHandlerFactory.Create(PCAxis.Web.Core.Enums.DatabaseType.CNMM).GetSelection(nodeId).Selection, true) == 0)
                                        {
                                            tblFix = tbl;
                                        }
                                    }
                                    if (String.IsNullOrEmpty(item.SortCode))
                                    {
                                        item.SortCode = item.Text;
                                    }
                                };
                                m.Restriction = item =>
                                {
                                    return true;
                                };
                            });


                if (tblFix != null)
                {
                    currentItem = tblFix;
                }
                else
                {
                    currentItem = menu.CurrentItem;
                }
                return menu;
            }
            catch (Exception e)
            {
                _logger.Error("Error in ConfigDatamodelMenu.Create.   dbid=" + dbid + " ,nodeId=" + nodeId  + "Error=" + e.Message + ". Creation of search index aborted.");
                currentItem = null;
                return null;
            }


        }

        /// <summary>
        /// Customize the table title
        /// </summary>
        /// <param name="tbl">TableLink object</param>
        /// <param name="showPublished">If published date shall be displayed or not</param>
        private static void CustomizeTableTitle(TableLink tbl, bool showPublished)
        {
            string published = "";

            if (showPublished)
            {
                published = tbl.Published.HasValue ? string.Format("<em> [{0}]</em>", tbl.Published.Value.ToShortDateString()) : "";
            }

            if (IsInteger(tbl.Text[tbl.Text.Length - 1].ToString()))
            {
                tbl.Text = string.Format("{0}{1}", tbl.Text, published);
            }
            else if (tbl.Text[tbl.Text.Length - 1].ToString() == "-") //Specialfall när titeln slutar med streck. Då ska bara slutår läggas till.
            {
                tbl.Text = string.Format("{0} {1}{2}", tbl.Text, tbl.EndTime, published);
            }
            else if (tbl.StartTime == tbl.EndTime)
            {
                tbl.Text = string.Format("{0} {1}{2}", tbl.Text, tbl.StartTime, published);
            }
            else
            {
                tbl.Text = string.Format("{0} {1} - {2}{3}", tbl.Text, tbl.StartTime, tbl.EndTime, published);
            }
        }

        /// <summary>
        /// Check if string is integer
        /// </summary>
        /// <param name="value"></param>
        /// <returns>True if integer, else false</returns>
        private static bool IsInteger(string value)
        {
            int outValue;

            return int.TryParse(value, out outValue);
        }

    }
}
