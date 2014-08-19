using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;

namespace ADAuthForOpenVPN
{
    class Program
    {
        private static string _username;
        private static string _password;
        private static string _domain;
        private static string _dn;
        private static List<string> _groups;

        private static EventLog _appLog;
        private static EventLog AppLog
        {
            get
            {
                return _appLog ?? (_appLog = new EventLog
                {
                    Source = "ADAuthForOpenVPN"
                });
            }
        }

        static void Main(string[] args)
        {
            if (args.Length == 1 && ReadCredentials(args[0]) && ReadSettings("ADAuthForOpenVPN.ini") && ValidateUser())
            {
                Environment.Exit(0);
            }

            Environment.Exit(1);
        }

        private static bool ReadCredentials(string filePath)
        {
            var success = false;

            try
            {
                using (var sr = new StreamReader(filePath))
                {
                    _username = sr.ReadLine();
                    _password = sr.ReadLine();
                }

                if (!string.IsNullOrEmpty(_username))
                {
                    success = true;
                }
            }
            catch (Exception ex)
            {
                AppLog.WriteEntry("Unable to read username and password from OpenVPN file. " + ex.GetBaseException().Message, EventLogEntryType.Error);
            }

            return success;
        }

        private static bool ReadSettings(string filePath)
        {
            var success = false;

            try
            {
                using (var sr = new StreamReader(filePath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.StartsWith("DOMAIN=", StringComparison.InvariantCultureIgnoreCase))
                        {
                            _domain = line.Substring("DOMAIN=".Length).Trim();
                        }
                        else if (line.StartsWith("DN=", StringComparison.InvariantCultureIgnoreCase))
                        {
                            _dn = line.Substring("DN=".Length).Trim();
                            if (_dn.StartsWith("\"") && _dn.EndsWith("\""))
                            {
                                _dn = _dn.Substring(1, _dn.Length - 1);
                            }
                        }
                        else if (line.StartsWith("GROUPS=", StringComparison.InvariantCultureIgnoreCase))
                        {
                            _groups = line.Substring("GROUPS=".Length).Split(',').ToList();
                            for (int i = 0; i < _groups.Count; i++)
                            {
                                _groups[i] = _groups[i].Trim();
                            }
                        }
                    }

                    if (_domain != null)
                    {
                        success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                AppLog.WriteEntry("Unable to read ini file. " + ex.GetBaseException().Message, EventLogEntryType.Error);
            }

            return success;
        }

        private static bool ValidateUser()
        {
            var isValid = false;

            try
            {
                var ctx = new PrincipalContext(ContextType.Domain, _domain, _dn);
                if (ctx.ValidateCredentials(_username, _password))
                {
                    var user = UserPrincipal.FindByIdentity(ctx, _username);
                    if (user != null)
                    {
                        if (_groups == null || !_groups.Any())
                        {
                            isValid = true;
                        }
                        else
                        {
                            var userGroups = user.GetGroups();
                            if (userGroups.Any(x => _groups.Contains(x.Name)))
                            {
                                isValid = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppLog.WriteEntry("Error connecting to domain: " + _domain + ". " + ex.GetBaseException().Message, EventLogEntryType.Error);
            }

            return isValid;
        }
    }
}
