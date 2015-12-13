using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace EventORM.Data
{
    public class RecordSet<T> : IQueryable<T>
    {
        //These fields are set with reflection, this may not be the best thing in the world but it works.
        private readonly string _connStr;
        private readonly string _contextName;

        public RecordSet()
        {
            Expression = Expression.Constant(this);
            Provider = new RecordSetQueryProvider<T>();
        }

        public RecordSet(RecordSetQueryProvider<T> provider, Expression expression)
        {
            Provider = provider;
            Expression = expression;
        }

        public void Add(T Entity)
        {
            var type = typeof(T);
            var name = type.Name;
            throw new NotImplementedException();
        }

        public int GetNextId()
        {
            try
            {
                var type = Expression.Type.GenericTypeArguments[0];
                var key = String.Format("{0}_{0}Id", type.Name);
                var query = String.Format("SELECT TOP 1 [{0}] + 1", key);
                query += String.Format("FROM [dbo].[{0}]", _contextName);
                query += String.Format("ORDER BY [{0}] DESC", key);
                int result;
                using (var cmd = new SqlCommand(query, new SqlConnection(_connStr)))
                {
                    cmd.Connection.Open();
                    result = (int)cmd.ExecuteScalar();
                    cmd.Connection.Close();
                }
                return result;
            }
            catch (Exception)
            {
                //Exception handling should be better but the basic reasoning
                //is because this failed there are no existing records so return 1
                return 1;
            }

        }

        public Type ElementType
        {
            get
            {
                return typeof(T);
            }
        }

        public Expression Expression { get; private set; }
        public IQueryProvider Provider { get; private set; }

        public IEnumerator<T> GetEnumerator()
        {
            return (Provider.Execute<IEnumerable<T>>(Expression)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (Provider.Execute<IEnumerable>(Expression)).GetEnumerator();
        }
    }
}
