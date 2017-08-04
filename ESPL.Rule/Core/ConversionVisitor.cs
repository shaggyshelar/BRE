using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Core
{
    /// <summary>
    /// A helper class. It replaces IndexOf expressions with Contains so that Linq-to-SQL can convert them to the LIKE statement.
    /// </summary>
    internal class ConversionVisitor : ExpressionVisitor
    {
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.NotEqual && node.Left.NodeType == ExpressionType.Call)
            {
                MethodCallExpression methodCallExpression = node.Left as MethodCallExpression;
                if (methodCallExpression.Method.Name == "IndexOf" && methodCallExpression.Arguments.Count == 2 && methodCallExpression.Arguments[1].NodeType == ExpressionType.Constant && (methodCallExpression.Arguments[1] as ConstantExpression).Type == typeof(StringComparison) && node.Right.NodeType == ExpressionType.Constant && (int)(node.Right as ConstantExpression).Value == -1)
                {
                    MethodInfo method = (from x in typeof(string).GetMethods()
                                         where x.Name == "Contains"
                                         select x).First<MethodInfo>();
                    return this.Visit(Expression.Call(methodCallExpression.Object, method, new Expression[]
					{
						methodCallExpression.Arguments[0]
					}));
                }
            }
            return base.VisitBinary(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression method)
        {
            if (method.Method.Name == "Equals" && method.Arguments.Count == 3 && method.Arguments[2].NodeType == ExpressionType.Constant && (method.Arguments[2] as ConstantExpression).Type == typeof(StringComparison))
            {
                return this.Visit(Expression.Call(typeof(string).GetMethod("Equals", new Type[]
				{
					typeof(string),
					typeof(string)
				}), method.Arguments[0], method.Arguments[1]));
            }
            if ((method.Method.Name == "StartsWith" || method.Method.Name == "EndsWith") && method.Arguments.Count == 2 && method.Arguments[1].NodeType == ExpressionType.Constant && (method.Arguments[1] as ConstantExpression).Type == typeof(StringComparison))
            {
                MethodInfo method2 = (from x in typeof(string).GetMethods()
                                      where x.Name == method.Method.Name
                                      select x).First<MethodInfo>();
                return this.Visit(Expression.Call(method.Object, method2, new Expression[]
				{
					method.Arguments[0]
				}));
            }
            return base.VisitMethodCall(method);
        }
    }
}
