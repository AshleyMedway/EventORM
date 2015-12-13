using System.Linq;
using System.Linq.Expressions;

namespace EventORM.Data
{
    internal class ExpressionTreeModifier<T> : ExpressionVisitor
    {
        private IQueryable<T> queryable;

        public ExpressionTreeModifier(IQueryable<T> queryable)
        {
            this.queryable = queryable;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            if (c.Type == typeof(RecordSet<T>))
                return Expression.Constant(queryable);
            else
                return c;
        }
    }
}