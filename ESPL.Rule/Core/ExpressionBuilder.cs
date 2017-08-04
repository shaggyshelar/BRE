using ESPL.Rule.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ESPL.Rule.Core
{
    internal class ExpressionBuilder : ExpressionBuilderBase
    {
        public ExpressionBuilder(Type sourceType, GetRuleInternalDelegate getRule)
            : base(sourceType, getRule)
        {
        }

        internal LambdaExpression GetPredicateExpression(XElement rule)
        {
            Expression safeExpressionBody = base.GetSafeExpressionBody(rule, false);
            return Expression.Lambda(safeExpressionBody, new ParameterExpression[]
			{
				this.source
			});
        }

        internal Delegate CompileRule(XElement rule)
        {
            LambdaExpression predicateExpression = this.GetPredicateExpression(rule);
            return predicateExpression.Compile();
        }

        internal Delegate CompileRule(LambdaExpression bodyExpression)
        {
            return bodyExpression.Compile();
        }
    }

    internal class ExpressionBuilder<TSource> : ExpressionBuilderBase
    {
        public ExpressionBuilder(GetRuleInternalDelegate getRule)
            : base(typeof(TSource), getRule)
        {
        }

        internal Expression<Func<TSource, bool>> GetPredicateExpression(XElement rule)
        {
            Expression safeExpressionBody = base.GetSafeExpressionBody(rule, false);
            return Expression.Lambda<Func<TSource, bool>>(safeExpressionBody, new ParameterExpression[]
			{
				this.source
			});
        }

        internal Func<TSource, bool> CompileRule(XElement rule)
        {
            Expression<Func<TSource, bool>> predicateExpression = this.GetPredicateExpression(rule);
            return predicateExpression.Compile();
        }

        internal Func<TSource, bool> CompileRule(Expression<Func<TSource, bool>> ruleExpression)
        {
            return ruleExpression.Compile();
        }
    }
}
