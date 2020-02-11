using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;

namespace Data.Abstractions
{
    public interface IDataSet<T> : IQueryable<T>, IEnumerable<T> where T : class
    {
        T Find([NotNull] params object[] keys);
        T Add([NotNull] T entity);
        IEnumerable<T> AddRange([NotNull] params T[] entities);
        T Remove([NotNull] T entity);
        T Remove([NotNull] params object[] keys);
        IEnumerable<T> RemoveRange([NotNull] params T[] entities);
    }
}