using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using TeleBilling_v02_.NavCustomerInfo;

namespace TeleBilling_v02_.Repository.Navision
{
    public interface ICustomerInfo2Repository:IDisposable
    {
        //CustomerInfo2_Service GetService(string username, string password, string domain);
        List<CustomerInfo2> GetCustomers();
        CustomerInfo2 GetCustomer(string customerCvr);
    }
    public class CustomerInfo2Repository: ICustomerInfo2Repository,IDisposable
    {
        private CustomerInfo2_Service service;
        public CustomerInfo2Repository(CustomerInfo2_Service service) { this.service = service; }

        
        //public CustomerInfo2_Service GetService(string username, string password, string domain)
        //{
        //    service = new CustomerInfo2_Service();
        //    service.Credentials = new NetworkCredential("rpnavapi", "Telefon1", "Gowingu");
        //    return service;
        //}
        public List<CustomerInfo2> GetCustomers()
        {
            return  service.ReadMultiple(new CustomerInfo2_Filter[] { }, null, 0).ToList();
        }

        public CustomerInfo2 GetCustomer(string customerCvr)
        {
            return service.Read(customerCvr);
        }

        #region IDisposable Support
        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    service.Dispose();
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