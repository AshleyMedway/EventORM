using System;
using System.Linq;
using System.Linq.Expressions;

namespace EventORM.Data
{
    public class RecordSetQueryProvider<T> : IQueryProvider
    {
        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = expression.Type;
            return (IQueryable)Activator.CreateInstance(typeof(RecordSet<>).MakeGenericType(elementType), new object[] { this, expression });
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var newProvider = new RecordSetQueryProvider<TElement>();
            return new RecordSet<TElement>(newProvider, expression);
        }

        public object Execute(Expression expression)
        {
            return RecordSetQueryContext.Execute<T>(expression, false);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            bool IsEnumerable = (typeof(TResult).Name == "IEnumerable`1");
            return (TResult)RecordSetQueryContext.Execute<T>(expression, IsEnumerable);
        }
    }
}
