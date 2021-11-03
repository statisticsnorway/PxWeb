using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PCAxis.Sql.DbConfig;
using System.Data.Common;

namespace PXWeb.Database
{
    //TODO: Consider moving to sql-nuget ?
    public class FormsDbStringProvider : IDbStringProvider
    {
        public string GetConnectionString(SqlDbConfig config, string user, string password)
        {
            string suser = config.Database.Connection.DefaultUser;
            string spassword = config.Database.Connection.DefaultPassword;

            
      
            //denne bør ligge i en logoutklasse eller membershipprovider
            //if (HttpContext.Current.Session != null)
            //{
            //    if (!HttpContext.Current.User.Identity.IsAuthenticated)
            //    {
            //        HttpContext.Current.Session["PXUSER"] = null;
            //        HttpContext.Current.Session["PXPASSWORD"] = null;
            //    }
            //}

            if (HttpContext.Current != null)
            {
                if (HttpContext.Current.Session != null)
                {
                    if (HttpContext.Current.Session["PXUSER"] != null && HttpContext.Current.Session["PXPASSWORD"] != null)
                    {
                        suser = HttpContext.Current.Session["PXUSER"] as string;
                        spassword = HttpContext.Current.Session["PXPASSWORD"] as string;
                    }
                }
            }
            //flytta denne if en ned hit fot å kunne logg på på nytt i samme sesjon
            if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
            {
                suser = user;
                spassword = password;
            }
            var db = config.Database;
            string tmp1 = string.Copy(db.Connection.ConnectionString);
            DbConnectionStringBuilder connBuilder = new DbConnectionStringBuilder();
            connBuilder.ConnectionString = tmp1;
            if (connBuilder.ContainsKey(db.Connection.KeyForPassword))
            {
                connBuilder.Remove(db.Connection.KeyForPassword);
                connBuilder.Add(db.Connection.KeyForPassword, spassword);
            }
            if (connBuilder.ContainsKey(db.Connection.KeyForUser))
            {
                connBuilder.Remove(db.Connection.KeyForUser);
                connBuilder.Add(db.Connection.KeyForUser, suser);
            }

            return connBuilder.ConnectionString;
        }
    }
}