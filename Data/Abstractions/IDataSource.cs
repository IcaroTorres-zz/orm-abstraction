using System;

namespace Data.Abstractions
{
    public interface IDataSource : IDisposable
    {
        IDataSource BeginTransaction();
        IDataSet<T> Set<T>() where T : class;
        void CommitState();
        void RollbackState();
        void CommitTransaction();
        void RollbackTransaction();
    }
}