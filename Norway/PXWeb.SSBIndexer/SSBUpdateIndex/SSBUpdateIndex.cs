using PCAxis.Search;
using PCAxis.Sql.DbConfig;

using System;
using System.Collections.Generic;
using System.Data;

namespace PXWeb.SSBIndexer.SSBUpdateIndex
{

    public class SSBUpdateIndex : ISearchIndex
    {

        private static log4net.ILog _logger;

        /// <summary>
        /// Get tables that have changed their metadata since the dateFrom date
        /// </summary>
        /// <param name="dateFrom"></param>
        /// <param name="database"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public List<TableUpdate> GetUpdatedTables(DateTime dateFrom, string database, string language)
        {
            _logger = log4net.LogManager.GetLogger(typeof(SSBUpdateIndex));
            List<string> MainTables = new List<string>();
            List<PCAxis.Search.TableUpdate> lst = new List<PCAxis.Search.TableUpdate>();
            PCAxis.Search.TableUpdate tbl;
            string sql = "";
            string sqlMenu;

            //tbl = new PCAxis.Search.TableUpdate();
            //tbl.Id = "AKUAarNY";
            //tbl.Path = "al/al03/aku/SBMENU420/AKUAarNY";
            //lst.Add(tbl);
            //return lst;

            SqlDbConfig mySqlDbConfig = SqlDbConfigsStatic.DataBases[database];

            var connestionString = mySqlDbConfig.GetDefaultConnString();
            //var metaOwner = mySqlDbConfig.MetaOwner;


            if (mySqlDbConfig.MetaModel == "2.4")
            {
                SqlDbConfig_24 DB = (SqlDbConfig_24)SqlDbConfigsStatic.DataBases[database];
                sql = "select distinct " + DB.ContentsTime.MainTableCol.Id() + " from " + DB.ContentsTime.GetNameAndAlias() + "," + DB.MainTable.GetNameAndAlias() + " where " + DB.ContentsTime.MainTableCol.Is(DB.MainTable.MainTableCol) + " AND " + DB.MainTable.TableStatusCol.Is("'A'") + " AND " + DB.MainTable.PresCategoryCol.Is("'O'") + " AND trunc(" + DB.ContentsTime.Alias + ".publ_dato) <=trunc(sysdate) and   trunc(" + DB.ContentsTime.Alias + ".publ_dato) > trunc(sysdate)-7";
                sqlMenu = "SELECT  " + DB.MenuSelection.MenuCol.Id() + " FROM " + DB.MenuSelection.GetNameAndAlias() + " CONNECT BY PRIOR " + DB.MenuSelection.MenuCol.Id() + " =  " + DB.MenuSelection.SelectionCol.Id() + " start with " + DB.MenuSelection.SelectionCol.Id() + " ='";
            }
            else if (mySqlDbConfig.MetaModel == "2.3")
            {
                SqlDbConfig_23 DB = (SqlDbConfig_23)SqlDbConfigsStatic.DataBases[database];
                sql = "select distinct " + DB.ContentsTime.MainTableCol.Id() + " from " + DB.ContentsTime.GetNameAndAlias() + "," + DB.MainTable.GetNameAndAlias() + " where " + DB.ContentsTime.MainTableCol.Is(DB.MainTable.MainTableCol) + " AND " + DB.MainTable.TableStatusCol.Is("'A'") + " AND " + DB.MainTable.PresCategoryCol.Is("'O'") + " AND trunc(" + DB.ContentsTime.Alias + ".publ_dato) <=trunc(sysdate) and   trunc(" + DB.ContentsTime.Alias + ".publ_dato) > trunc(sysdate)-7";
                sqlMenu = "SELECT  " + DB.MenuSelection.MenuCol.Id() + " FROM " + DB.MenuSelection.GetNameAndAlias() + " CONNECT BY PRIOR " + DB.MenuSelection.MenuCol.Id() + " =  " + DB.MenuSelection.SelectionCol.Id() + " start with " + DB.MenuSelection.SelectionCol.Id() + " ='";
            }
            else if (mySqlDbConfig.MetaModel == "2.2")
            {
                SqlDbConfig_22 DB = (SqlDbConfig_22)SqlDbConfigsStatic.DataBases[database];
                sql = "select distinct " + DB.ContentsTime.MainTableCol.Id() + " from " + DB.ContentsTime.GetNameAndAlias() + "," + DB.MainTable.GetNameAndAlias() + " where " + DB.ContentsTime.MainTableCol.Is(DB.MainTable.MainTableCol) + " AND " + DB.MainTable.TableStatusCol.Is("'A'") + " AND " + DB.MainTable.PresCategoryCol.Is("'O'") + " AND trunc(" + DB.ContentsTime.Alias + ".publ_dato) <=trunc(sysdate) and   trunc(" + DB.ContentsTime.Alias + ".publ_dato) > trunc(sysdate)-7";
                sqlMenu = "SELECT  " + DB.MenuSelection.MenuCol.Id() + " FROM " + DB.MenuSelection.GetNameAndAlias() + " CONNECT BY PRIOR " + DB.MenuSelection.MenuCol.Id() + " =  " + DB.MenuSelection.SelectionCol.Id() + " start with " + DB.MenuSelection.SelectionCol.Id() + " ='";
            }
            else
            {
                SqlDbConfig_21 DB = (SqlDbConfig_21)SqlDbConfigsStatic.DataBases[database];
                sql = "select distinct " + DB.ContentsTime.MainTableCol.Id() + " from " + DB.ContentsTime.GetNameAndAlias() + "," + DB.MainTable.GetNameAndAlias() + " where " + DB.ContentsTime.MainTableCol.Is(DB.MainTable.MainTableCol) + " AND " + DB.MainTable.TableStatusCol.Is("'A'") + " AND " + DB.MainTable.PresCategoryCol.Is("'O'") + " AND trunc(" + DB.ContentsTime.Alias + ".publ_dato) <=trunc(sysdate) and   trunc(" + DB.ContentsTime.Alias + ".publ_dato) > trunc(sysdate)-7";
                sqlMenu = "SELECT  " + DB.MenuSelection.MenuCol.Id() + " FROM " + DB.MenuSelection.GetNameAndAlias() + " CONNECT BY PRIOR " + DB.MenuSelection.MenuCol.Id() + " =  " + DB.MenuSelection.SelectionCol.Id() + " start with " + DB.MenuSelection.SelectionCol.Id() + " ='";
            }

            //sholud we make a mySqlDbConfig.GetPxSqlCommand() ?

            InfoForDbConnection dbInfo = mySqlDbConfig.GetInfoForDbConnection(connestionString);
            try
            {

                using (PCAxis.Sql.PxSqlCommand myCommand = new PCAxis.Sql.DbClient.PxSqlCommandForTempTables(dbInfo.DataBaseType, dbInfo.DataProvider, dbInfo.ConnectionString))
                {

                    //sql = "select distinct " + DB.ContentsTime.MainTableCol.Id() + " from " + DB.ContentsTime.GetNameAndAlias() + "," + DB.MainTable.GetNameAndAlias() + " where " + DB.ContentsTime.MainTableCol.Is(DB.MainTable.MainTableCol) + " AND " + DB.MainTable.TableStatusCol.Is("'A'") + " AND " + DB.MainTable.PresCategoryCol.Is("'O'") + " AND trunc(" + DB.ContentsTime.Alias + ".publ_dato) <=trunc(sysdate) and   trunc(" + DB.ContentsTime.Alias + ".publ_dato) > trunc(sysdate)-7";
                    // sql = "select distinct it.huvudtabell from " + metaOwner + "innehalltid it," + metaOwner + "huvudtabell h where it.huvudtabell=h.huvudtabell and  Upper(h.tabellstatus)='A' and Upper(h.Preskategori) ='O' and  trunc(it.publ_dato) <=trunc(sysdate) and   trunc(it.publ_dato) > trunc(sysdate)-7";
                    _logger.Info("Kjører følgende sql for å finne nye tabeller: " + sql);
                    DataSet ds = myCommand.ExecuteSelect(sql);
                    DataRowCollection myRows = ds.Tables[0].Rows;

                    foreach (DataRow sqlRow in myRows)
                    {
                        string tmpMaintable = sqlRow[0].ToString();
                        MainTables.Add(tmpMaintable);
                        _logger.Info("Index skal oppdateres for tabell:  " + tmpMaintable);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.Message + " sql=" + sql);
                _logger.Error("Innerexception message: " + e.InnerException.Message);
                _logger.Error("Innerexception source: " + e.Source);
                _logger.Error("Innerexception stacktrace: " + e.InnerException.StackTrace);
            }

            try
            {

                using (PCAxis.Sql.PxSqlCommand myCommand = new PCAxis.Sql.DbClient.PxSqlCommandForTempTables(dbInfo.DataBaseType, dbInfo.DataProvider, dbInfo.ConnectionString))
                {
                  
                    foreach (string maintable in MainTables)
                    {
                        //    sql = "SELECT meny FROM " + metaOwner + "menyval2  CONNECT BY PRIOR meny = val start with val='" + maintable + "'";
                        sql = sqlMenu + maintable + "'";

                        string tmpPath = "";
                        int count = 0;
                        string rootMenu = "";

                        DataSet ds = myCommand.ExecuteSelect(sql);
                        DataRowCollection myRows = ds.Tables[0].Rows;

                        foreach (DataRow sqlRow in myRows)
                        {
                            if (count == 0)
                            {
                                rootMenu = sqlRow[0].ToString();
                            }
                            else
                            {
                                if (sqlRow[0].ToString().ToUpper().Equals("START"))
                                {
                                    tbl = new PCAxis.Search.TableUpdate();
                                    tbl.Id = maintable;
                                    tbl.Path = tmpPath.TrimStart('/') + "/" + rootMenu;  //Test er norsk og svensk meny forskjellig bygd opp???
                                                                                         // tbl.Path = tmpPath.TrimStart('/');
                                    lst.Add(tbl);
                                    tmpPath = "";
                                }
                                else
                                {
                                    tmpPath = "/" + sqlRow[0].ToString() + tmpPath;
                                }
                            }
                            count++;
                        }

                    }
                }
            }

            catch (Exception e)
            {
                _logger.Error(e.Message + " sql=" + sql);
            }

            return lst;
        }
    }
}
