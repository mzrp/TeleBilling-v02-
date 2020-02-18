using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TeleBilling_v02_.Models;

namespace TeleBilling_v02_.Repository
{
    public interface IAgreementRepository : IDisposable
    {
        IEnumerable<Agreement> GetAgreements();
        IEnumerable<Agreement> GetAgreements(int csvFileId);
        IEnumerable<ZoneRecords> GetAgreementZones(int agreementId);
        IEnumerable<ZoneRecords> GetFileZones(int csvFileId);
        IEnumerable<Supplier> GetSuppliers();
        //IEnumerable<Supplier> GetSuppliers(string supplierType);

        Agreement GetAgreement(int agreementId);
    }

    public class AgreementRepository : IAgreementRepository, IDisposable
    {
        DBModelsContainer db;

        public AgreementRepository(DBModelsContainer db) { this.db = db; }

        public IEnumerable<Agreement> GetAgreements()
        {
            return db.AgreementSet.ToList();
        }

        public IEnumerable<Agreement> GetAgreements(int csvFileId)
        {
            return db.AgreementSet.Where(x => x.CSVFileId == csvFileId).ToList();
        }

        public Agreement GetAgreement(int agreementId)
        {
            return db.AgreementSet.Where(a => a.Id == agreementId).FirstOrDefault();
        }

        public IEnumerable<ZoneRecords> GetAgreementZones(int agreementId)
        {
            return db.ZoneRecordsSet.Where(z => z.AgreementId == agreementId).ToList();
        }

        public IEnumerable<ZoneRecords> GetFileZones(int csvFileId)
        {
            return db.ZoneRecordsSet.Where(z => z.CSVFileId == csvFileId).ToList();
        }

        public IEnumerable<Supplier> GetSuppliers()
        {
            return db.SupplierSet.ToList<Supplier>();
        }

        //public IEnumerable<Supplier> GetSuppliers(string supplierType)
        //{
        //    return db.SupplierSet.Where(s => s.Type == supplierType).ToList<Supplier>();
        //}

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