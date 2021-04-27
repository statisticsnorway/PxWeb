using PCAxis.Sql.DbClient;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace PXWeb.Security
{
    public class CNMMMembershipProvider : MembershipProvider
    {

        private string _dbid;
        private string _applicationName;
        private bool _enablePasswordReset;
        private bool _enablePasswordRetrieval;
        private int _maxInvalidPasswordAttempts;
        private int _passwordAttemptWindow;
        private bool _requiresQuestionAndAnswer;
        private int _minRequiredNonAlphanumericCharacters;
        private int _minRequiredPasswordLength;
        private MembershipPasswordFormat _passwordFormat;
        private string _passwordStrengthRegularExpression;
        private bool _requiresUniqueEmail;

        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            if (name == null || name.Length == 0)
                name = "CNMMMembershipProvider";

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "SSB CNMM Membership provider");
            }

            // Initialize the abstract base class. 
            base.Initialize(name, config);

            _dbid = config["dbid"];
            _applicationName = GetConfigValue(config["applicationName"], "pxweb");
            _enablePasswordReset = Convert.ToBoolean(GetConfigValue(config["enablePasswordReset"], "false"));
            _enablePasswordRetrieval = Convert.ToBoolean(GetConfigValue(config["enablePasswordRetrieval"], "false"));
            _maxInvalidPasswordAttempts = Convert.ToInt32(GetConfigValue(config["maxInvalidPasswordAttempts"], "5"));
            _passwordAttemptWindow = Convert.ToInt32(GetConfigValue(config["passwordAttemptWindow"], "10"));
            _requiresQuestionAndAnswer = Convert.ToBoolean(GetConfigValue(config["requiresQuestionAndAnswer"], "false"));
            _minRequiredPasswordLength = Convert.ToInt32(GetConfigValue(config["minRequiredPasswordLength"], "7"));
            _minRequiredNonAlphanumericCharacters = Convert.ToInt32(GetConfigValue(config["minRequiredNonAlphanumericCharacters"], "1"));
            _passwordStrengthRegularExpression = Convert.ToString(GetConfigValue(config["passwordStrengthRegularExpression"], ""));
            _requiresUniqueEmail = Convert.ToBoolean(GetConfigValue(config["requiresUniqueEmail"], "true"));

            string temp_format = config["passwordFormat"];
            if (temp_format == null)
            {
                temp_format = "Hashed";
            }

            switch (temp_format)
            {
                case "Hashed":
                    _passwordFormat = MembershipPasswordFormat.Hashed;
                    break;
                case "Encrypted":
                    _passwordFormat = MembershipPasswordFormat.Encrypted;
                    break;
                case "Clear":
                    _passwordFormat = MembershipPasswordFormat.Clear;
                    break;
                default:
                    throw new ProviderException("Password format not supported.");

            }
        }

        private string GetConfigValue(string configValue, string defaultValue)
        {
            if (String.IsNullOrEmpty(configValue))
                return defaultValue;

            return configValue;
        }

    

        public override bool ValidateUser(string username, string password)
        {
            bool isValid = false;
            try
            {
  
                var dbConf = PCAxis.Sql.DbConfig.SqlDbConfigsStatic.DataBases[_dbid];
                var dbInfo = dbConf.GetInfoForDbConnection(username, password);

                using (var cmd = new PxSqlCommandForTempTables(dbInfo.DataBaseType, dbInfo.DataProvider, dbInfo.ConnectionString))
                {
                    cmd.ExecuteSelect("select 1 from dual");
                }
                
                isValid = true;

                HttpContext.Current.Session.Add("PXUSER", username);
                HttpContext.Current.Session.Add("PXPASSWORD", password);
            }
            catch (Exception ex)
            {
                //TODO log exception
            }
            


            return isValid;
        }

        public override bool ChangePassword(string username, string oldPwd, string newPwd)
        {
            throw new NotImplementedException();
        }

        public override bool ChangePasswordQuestionAndAnswer(string username,
                 string password,
                 string newPwdQuestion,
                 string newPwdAnswer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser CreateUser(string username,
             string password,
             string email,
             string passwordQuestion,
             string passwordAnswer,
             bool isApproved,
             object providerUserKey,
             out MembershipCreateStatus status)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override int GetNumberOfUsersOnline()
        {
            return 0;
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        public override string GetUserNameByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override bool UnlockUser(string username)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }
        public override string ApplicationName
        {
            get { return _applicationName; }
            set { _applicationName = value; }
        }

        public override bool EnablePasswordReset
        {
            get { return _enablePasswordReset; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return _enablePasswordRetrieval; }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return _maxInvalidPasswordAttempts; }
        }

        public override int PasswordAttemptWindow
        {
            get { return _passwordAttemptWindow; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return _minRequiredNonAlphanumericCharacters; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return _minRequiredPasswordLength; }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { return _passwordFormat; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return _passwordStrengthRegularExpression; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return _requiresQuestionAndAnswer; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return _requiresUniqueEmail; }
        }


    }
}