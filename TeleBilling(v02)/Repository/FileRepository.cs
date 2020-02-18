using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using TeleBilling_v02_.Models;
using Type = TeleBilling_v02_.Models.Type;

namespace TeleBilling_v02_.Repository
{
    public interface IFileRepository : IDisposable
    {
        IEnumerable<CSVFile> GetCsvFileByTypeId(int typeId);
        //IEnumerable<CSVFile> GetInvoiceFiles(int typeId);
        //IEnumerable<CSVFile> GetPriceFiles(int typeId);        
        IEnumerable<InvoiceRecords> GetFileInvoiceDetails(int fileId);
        IEnumerable<ZoneRecords> GetFileZoneDetails(int fileId);
        bool CreateFile(CSVFile csvFile);
        IEnumerable<Supplier> GetSuppliers();
        
        CSVFile GetFileByName(string filename);
        CSVFile GetFileById(int fileId);
        CSVFile GetFileBySupplierID(int supplierId, int typeId);
        Type GetType(string typeName);
        User GetUser(string username);
        Supplier GetSupplier(int supplierId);

    }

    public class FileRepository : IFileRepository, IDisposable
    {
        private DBModelsContainer db;
        public FileRepository(DBModelsContainer db) { this.db = db; }

        public IEnumerable<CSVFile> GetCsvFileByTypeId(int typeId)
        {
            return db.CSVFileSet.Where(f => f.TypeId == typeId).ToList<CSVFile>();
        }

        //public IEnumerable<CSVFile> GetInvoiceFiles(int typeId)
        //{
        //    return db.CSVFileSet.Where(f => f.TypeId == typeId).ToList();
        //}

        //public IEnumerable<CSVFile> GetPriceFiles(int typeId)
        //{
        //    return db.CSVFileSet.Where(f => f.TypeId == typeId).ToList();
        //}

        public CSVFile GetFileBySupplierID(int supplierId, int typeId)
        {
            return db.CSVFileSet.SingleOrDefault(f => f.SupplierId== supplierId && f.TypeId== typeId);
        }

        public bool CreateFile(CSVFile csvFile)
        {
            try
            {
                db.CSVFileSet.Add(csvFile);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public CSVFile GetFileByName(string filename)
        {
            return db.CSVFileSet.Where(x => x.Name == filename).FirstOrDefault();
        }
        public CSVFile GetFileById(int fileId)
        {
            return db.CSVFileSet.Where(x => x.Id == fileId).FirstOrDefault();
        }
        
        public IEnumerable<InvoiceRecords> GetFileInvoiceDetails(int fileId)
        {
            return db.InvoiceRecordsSet.Where(x => x.CSVFileId == fileId).ToList();
        }

        public IEnumerable<ZoneRecords> GetFileZoneDetails(int fileId)
        {
            return db.ZoneRecordsSet.Where(x => x.CSVFileId == fileId).ToList();
        }

        public Type GetType(string typeName)
        {
            return db.TypeSet.Where(x => x.Name == typeName).FirstOrDefault();
        }

        public User GetUser(string username)
        {
            return db.UserSet.Where(x => x.Name == username).FirstOrDefault();
        }

        public IEnumerable<Supplier> GetSuppliers()
        {
            return db.SupplierSet.ToList<Supplier>();
        }

        public Supplier GetSupplier(int supplierId)
        {
            return db.SupplierSet.Where(x => x.Id == supplierId).FirstOrDefault();
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