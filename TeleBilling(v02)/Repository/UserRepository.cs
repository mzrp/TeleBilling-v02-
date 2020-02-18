using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using TeleBilling_v02_.Models;

namespace TeleBilling_v02_.Repository
{
    public interface IUserRepository :IDisposable
    {
        User GetUser(string username);
        Dictionary<bool, string> Authenticat(string username, string password);
    }

    public class UserRepository: IUserRepository, IDisposable 
    {
        private DBModelsContainer db;
        public UserRepository(DBModelsContainer db) { this.db = db; }

        public User GetUser(string username)
        {
            return db.UserSet.Where(x => x.Name == username).FirstOrDefault();
        }

        public Dictionary<bool, string> Authenticat(string username, string password)
        {
            bool authenticated = false;
            string exception = string.Empty;

            string strServerName = "rpad02";
            string strBaseDN = "DC=Rackpeople,DC=dk";
            string strUserDN = "OU=Medarbejdere";

            string srvr = "LDAP://" + strServerName + "/" + strUserDN + "," + strBaseDN;
            try
            {
                DirectoryEntry entry = new DirectoryEntry(srvr, username, password);
                object nativeObject = entry.NativeObject;
                authenticated = true;
            }
            catch (DirectoryServicesCOMException cex)
            {
                //not authenticated; reason why is in cex  
                exception = cex.ToString();
            }
            catch (Exception ex)
            {
                //not authenticated due to some other exception [this is optional] 
                exception = ex.Message.ToString();
            }
            return new Dictionary<bool, string> { { authenticated, exception } };
        }

        #region IDisposable Support
        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    db.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}