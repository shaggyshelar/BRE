using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Core
{
    /// <summary>
    /// A struct holding a compiled rule and its expression tree.
    /// </summary>
    internal struct Predicate
    {
        /// <summary>
        /// A method which is result of compiling a rule. Calling this method will evaluate the rule.
        /// </summary>
        public Delegate Delegate;

        /// <summary>
        /// An expression tree in the form of lambda expression. 
        /// This predicate is similar to the one used in the System.Linq.Queryable.Where extension.
        /// </summary>
        public LambdaExpression Expression;
    }

    /// <summary>
    /// A generic version of the predicate, holding a compiled rule and its expression tree.
    /// </summary>
    /// <typeparam name="TSource">A type of the underlying source object</typeparam>
    internal struct Predicate<TSource>
    {
        /// <summary>
        /// A method which is result of compiling a rule. Calling this method will evaluate the rule.
        /// </summary>
        public Func<TSource, bool> Delegate;

        /// <summary>
        /// An expression tree in the form of lambda expression. 
        /// This predicate is similar to the one used in the System.Linq.Queryable.Where extension.
        /// </summary>
        public Expression<Func<TSource, bool>> Expression;
    }
}
