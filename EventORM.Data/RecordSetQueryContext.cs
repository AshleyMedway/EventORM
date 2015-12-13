using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace EventORM.Data
{
    internal class RecordSetQueryContext
    {
        internal static object Execute<T>(Expression expression, bool IsEnumerable)
        {
            var type = expression.Type;

            if (!IsQueryOverDataSource(expression))
                throw new InvalidProgramException("No query over the data source was specified.");

            var whereFinder = new InnermostWhereFinder();
            var whereExpression = whereFinder.GetInnermostWhere(expression);
            var lambdaExpression = (LambdaExpression)((UnaryExpression)(whereExpression.Arguments[1])).Operand;

            lambdaExpression = (LambdaExpression)Evaluator.PartialEval(lambdaExpression);

            var data = GetData<T>();

            var queryable = data.AsQueryable();

            var treeCopier = new ExpressionTreeModifier<T>(queryable);
            var newExpressionTree = treeCopier.Visit(expression);

            if (IsEnumerable)
                return queryable.Provider.CreateQuery(newExpressionTree);
            else
                return queryable.Provider.Execute(newExpressionTree);
        }

        private static T[] GetData<T>()
        {
            var type = typeof(T);
            var query = "SELECT DISTINCT";
            foreach (var prop in type.GetProperties())
            {
                if (prop.GetMethod.IsVirtual)
                    continue;

                query += String.Format(" [{0}],", prop.Name);
            }

            query = query.TrimEnd(',');
            query += " FROM[dbo].[ApplicationContext]";

            var data = new List<T>();
            using (var cmd = new SqlCommand(query, new SqlConnection("")))
            {
                cmd.Connection.Open();
                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var row = Activator.CreateInstance<T>();
                        for (int x = 0; x < reader.FieldCount; x++)
                        {
                            var propName = reader.GetDataTypeName(x);
                            var propInfo = type.GetProperty(propName);
                            var propValue = reader.GetValue(x);
                            propInfo.SetValue(row, Convert.ChangeType(propValue, propInfo.PropertyType), null);
                        }
                        data.Add(row);
                    }
                }
                reader.Close();
                cmd.Connection.Close();
            }
            return data.ToArray();
        }

        private static bool IsQueryOverDataSource(Expression expression)
        {
            return (expression is MethodCallExpression);
        }
    }
}