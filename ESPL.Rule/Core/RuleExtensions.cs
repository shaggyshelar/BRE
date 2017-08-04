using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ESPL.Rule.Core
{
    /// <summary>
    /// Provides a set of static methods for evaluating business rules against individual objects and for
    /// filtering objects that implement one of the following: <see cref="T:System.Collections.IEnumerable" />, <see cref="T:System.Collections.Generic.IEnumerable`1" />,
    /// <see cref="T:System.Linq.IQueryable" />, <see cref="T:System.Linq.IQueryable`1" />.
    /// <remarks>These methods use either <see cref="T:CodeEffects.Rule.Core.Evaluator`1" /> or <see cref="T:CodeEffects.Rule.Core.Evaluator" /> class internally
    /// to apply business rules.</remarks>
    /// </summary>
    public static class RuleExtensions
    {
        /// <summary>
        /// Filters the collection of source objects by applying a business rule. The rulesetXml is loaded into <see cref="T:CodeEffects.Rule.Core.Evaluator" />
        /// and its rules are compiled and stored in memory. Then either the first rule in the ruleset or the one referenced by the rule id is evaluated
        /// against each object in the collection. Those source objects that pass the rule are returned as a new
        /// collection of type <see cref="T:System.Collections.IEnumerable" />.
        /// </summary>
        /// <param name="source">An enumerable collection of type <see cref="T:System.Collections.Generic.IEnumerable`1" /> that needs to be filtered.</param>
        /// <param name="type">An underlying source type of an enumerable collection.</param>
        /// <param name="rulesetXml">The XML containing one or more rules to be evaluated against source objects.
        /// The XML ruleset must validate against http://codeeffects.com/schemas/rule/4 schema.</param>
        /// <param name="ruleId">An optional id of a rule. If there are more than one rule in the ruleset, you may evaluate a specific
        /// rule by providing its id. <remarks>Not to be confused with external reusable rules referenced in rules inside of the rulesetXml.
        /// The rule with this id must be present as a top level rule inside the rulesetXml or exception will be thrown.</remarks></param>
        /// <param name="getRule">The delegate to a method used to retrieve external rules by id (reusable rules referenced by other rules).
        /// If this parameter is null, the Evaluator will attempt to find any referenced rules inside the rulesetXml. If none are found
        /// an exception will be thrown.</param>
        /// <returns>Returns a new <see cref="T:System.Collections.Generic.IEnumerable`1" /> collection of source objects that passed the rule evaluation.</returns>
        public static IEnumerable Filter(this IEnumerable source, Type type, string rulesetXml, string ruleId = null, GetRuleDelegate getRule = null)
        {
            Evaluator evaluator = new Evaluator(type, rulesetXml, getRule, -1);
            List<object> list = new List<object>();
            Vector.DelayIfDemo();
            evaluator.SuspendDemoDelay = true;
            foreach (object current in source)
            {
                if (evaluator.Evaluate(current, ruleId))
                {
                    list.Add(current);
                }
            }
            return list;
        }

        /// <summary>
        /// Filters the collection of source objects by applying a business rule. The rulesetXml is loaded into <see cref="T:CodeEffects.Rule.Core.Evaluator`1" />
        /// and its rules are compiled and stored in memory. Then either the first rule in the ruleset or the one referenced by the rule id is evaluated
        /// against each object in the collection. Those source objects that pass the rule are returned as a new
        /// collection of type <see cref="T:System.Collections.Generic.IEnumerable`1" />.
        /// </summary>
        /// <typeparam name="TSource">An underlying source type of a generic enumerable collection.</typeparam>
        /// <param name="source">An enumerable collection of type <see cref="T:System.Collections.Generic.IEnumerable`1" /> that needs to be filtered.</param>
        /// <param name="rulesetXml">The XML containing one or more rules to be evaluated against source objects.
        /// The XML ruleset must validate against http://codeeffects.com/schemas/rule schema.</param>
        /// <param name="ruleId">An optional id of a rule. If there are more than one rule in the ruleset, you may evaluate a specific
        /// rule by providing its id. <remarks>Not to be confused with external reusable rules referenced in rules inside of the rulesetXml.
        /// The rule with this id must be present as a top level rule inside the rulesetXml or exception will be thrown.</remarks></param>
        /// <param name="getRule">The delegate to a method used to retrieve external rules by id (reusable rules referenced by other rules).
        /// If this parameter is null, the Evaluator will attempt to find any referenced rules inside the rulesetXml. If none are found
        /// an exception will be thrown.</param>
        /// <returns>Returns a new <see cref="T:System.Collections.Generic.IEnumerable`1" /> collection of source objects that passed the rule evaluation.</returns>
        public static IEnumerable<TSource> Filter<TSource>(this IEnumerable<TSource> source, string rulesetXml, string ruleId = null, GetRuleDelegate getRule = null)
        {
            Evaluator<TSource> evaluator = new Evaluator<TSource>(rulesetXml, getRule, -1);
            Func<TSource, bool> predicate = evaluator.GetPredicate(ruleId);
            Vector.DelayIfDemo();
            evaluator.SuspendDemoDelay = true;
            return source.Where(predicate);
        }

        /// <summary>
        /// Filters the collection of source objects by applying a business rule. The rulesetXml is loaded into <see cref="T:CodeEffects.Rule.Core.Evaluator`1" />
        /// and its rules are compiled and stored in memory. Then either the first rule in the ruleset or the one referenced by the rule id is evaluated
        /// against each object in the collection. Those source objects that pass the rule are returned as a new
        /// collection of type <see cref="T:System.Collections.Generic.IEnumerable`1" />.
        /// </summary>
        /// <typeparam name="TSource">An underlying source type of a generic enumerable collection.</typeparam>
        /// <param name="source">An enumerable collection of type <see cref="T:System.Collections.Generic.IEnumerable`1" /> that needs to be filtered.</param>
        /// <param name="rulesetXml">The XML containing one or more rules to be evaluated against source objects.
        /// The XML ruleset must validate against http://codeeffects.com/schemas/rule schema.</param>
        /// <param name="ruleIndex">A zero-based index of a rule to be executed. If there are more than one rule in the ruleset, you may evaluate a specific
        /// rule by providing its index. <remarks>Not to be confused with external reusable rules referenced in rules inside of the rulesetXml.
        /// The rule with this id must be present as a top level rule inside the rulesetXml or exception will be thrown.</remarks></param>
        /// <param name="getRule">The delegate to a method used to retrieve external rules by id (reusable rules referenced by other rules).
        /// If this parameter is null, the Evaluator will attempt to find any referenced rules inside the rulesetXml. If none are found
        /// an exception will be thrown.</param>
        /// <returns>Returns a new <see cref="T:System.Collections.Generic.IEnumerable`1" /> collection of source objects that passed the rule evaluation.</returns>
        [Obsolete("Consider using a Filter overload with the ruleId parameter instead.")]
        public static IEnumerable<TSource> Filter<TSource>(this IEnumerable<TSource> source, string rulesetXml, int ruleIndex, GetRuleDelegate getRule = null)
        {
            XElement xElement = XElement.Parse(rulesetXml);
            if (source == null || xElement == null)
            {
                throw new ArgumentNullException();
            }
            XNamespace ns = xElement.GetDefaultNamespace();
            IEnumerable<XElement> source2 = from x in xElement.Element(ns + "codeeffects").Elements()
                                            where x.Name == ns + "rule"
                                            select x;
            if (ruleIndex >= source2.Count<XElement>() || ruleIndex < 0)
            {
                throw new IndexOutOfRangeException("Rule index is out of range.");
            }
            string text = (string)source2.ElementAt(ruleIndex).Attribute("id");
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException("Rule must have non-empty 'id' attribute.");
            }
            return source.Filter(rulesetXml, text, getRule);
        }

        /// <summary>
        /// Filters a collection of source objects by applying a business rule. The rulesetXml is loaded into <see cref="T:CodeEffects.Rule.Core.Evaluator" />
        /// and its rules are compiled into expression trees. This method does not evaluate immediately. Instead it returns an expression tree which can be further
        /// modified. During immediate evaluation, the expression tree is translated into a set of backend commands supported by its LINQ provider.
        /// In case of LINQ-to-SQL, the expression tree is translated into a set of SQL commands.
        /// </summary>
        /// <param name="source">A queryable collection of type <see cref="T:System.Linq.IQueryable`1" /> that needs to be filtered.</param>
        /// <param name="type">An underlying source type of a queryable collection.</param>
        /// <param name="rulesetXml">The XML containing one or more rules to be evaluated against source objects.
        /// The XML ruleset must validate against http://codeeffects.com/schemas/rule/4 schema.</param>
        /// <param name="parameters">An set of various parameters that determine how to build and evalute the rule. See <see cref="T:CodeEffects.Rule.Core.EvaluationParameters" /> for more information.</param>
        /// <returns>Returns an expression tree <see cref="T:System.Linq.IQueryable`1" /> for deferred or immediate evaluation. Immediate evaluation is handled by a LINQ provider.</returns>
        public static IQueryable Filter(this IQueryable source, Type type, string rulesetXml, EvaluationParameters parameters)
        {
            if (source == null || string.IsNullOrWhiteSpace(rulesetXml))
            {
                throw new ArgumentNullException();
            }
            Evaluator evaluator = new Evaluator(type, rulesetXml, parameters);
            LambdaExpression lambdaExpression = evaluator.GetPredicateExpression(parameters.RuleId);
            LinqProviderType linqProviderType = parameters.LINQProviderType;
            if (linqProviderType == LinqProviderType.Default)
            {
                linqProviderType = RuleExtensions.GetProviderType(source);
            }
            if (linqProviderType == LinqProviderType.SQL || linqProviderType == LinqProviderType.Entities)
            {
                lambdaExpression = (LambdaExpression)RuleExtensions.ReplaceIndexOfMethod(lambdaExpression);
            }
            Vector.DelayIfDemo();
            evaluator.SuspendDemoDelay = true;
            typeof(IQueryable<>).MakeGenericType(new Type[]
			{
				type
			});
            IEnumerable<MethodInfo> source2 = typeof(Queryable).GetMethods().Where(delegate(MethodInfo method)
            {
                if (method.Name == "Where")
                {
                    return method.GetParameters().Where(delegate(ParameterInfo parameter)
                    {
                        if (parameter.ParameterType.Name.StartsWith("Expression"))
                        {
                            return (from genericArg in parameter.ParameterType.GetGenericArguments()
                                    where genericArg.GetGenericArguments().Count<Type>() == 2
                                    select genericArg).Any<Type>();
                        }
                        return false;
                    }).Any<ParameterInfo>();
                }
                return false;
            });
            MethodInfo methodInfo = source2.FirstOrDefault<MethodInfo>();
            Expression expression = Expression.Call(null, methodInfo.MakeGenericMethod(new Type[]
			{
				type
			}), new Expression[]
			{
				source.Expression,
				Expression.Quote(lambdaExpression)
			});
            return source.Provider.CreateQuery(expression);
        }

        /// <summary>
        /// Filters the collection of source objects by applying a business rule. The rulesetXml is loaded into <see cref="T:CodeEffects.Rule.Core.Evaluator" />
        /// and its rules are compiled into expression trees. This method does not evaluate immediately. Instead it returns an expression tree which can be further
        /// modified. During immediate evaluation, the expression tree is translated into a set of backend commands supported by its LINQ provider.
        /// In case of LINQ-to-SQL, the expression tree is translated into a set of SQL commands.
        /// </summary>
        /// <param name="source">A queryable collection of type <see cref="T:System.Linq.IQueryable" /> that needs to be filtered.</param>
        /// <param name="type">An underlying source type of a generic queryable collection.</param>
        /// <param name="rulesetXml">The XML containing one or more rules to be evaluated against source objects.
        /// The XML ruleset must validate against http://codeeffects.com/schemas/rule/4 schema.</param>
        /// <param name="ruleId">An optional id of a rule. If there are more than one rule in the ruleset, you may evaluate a specific
        /// rule by providing its id. <remarks>Not to be confused with external reusable rules referenced in rules inside of the rulesetXml.
        /// The rule with this id must be present as a top level rule inside the rulesetXml or an exception will be thrown.</remarks></param>
        /// <param name="getRule">The delegate to a method used to retrieve external rules by id (reusable rules referenced by other rules).
        /// If this parameter is null, the Evaluator will attempt to find any referenced rules inside the rulesetXml. If none are found
        /// an exception will be thrown.</param>
        /// <returns>Returns an expression tree <see cref="T:System.Linq.IQueryable`1" /> for deferred or immediate evaluation. Immediate evaluation is handled by a LINQ provider.</returns>
        public static IQueryable Filter(this IQueryable source, Type type, string rulesetXml, string ruleId = null, GetRuleDelegate getRule = null)
        {
            EvaluationParameters parameters = new EvaluationParameters
            {
                RuleId = ruleId,
                RuleGetter = getRule,
                LINQProviderType = LinqProviderType.Default,
                PerformNullChecks = false
            };
            return source.Filter(type, rulesetXml, parameters);
        }

        /// <summary>
        /// Filters a collection of source objects by applying a business rule. The rulesetXml is loaded into <see cref="T:CodeEffects.Rule.Core.Evaluator`1" />
        /// and its rules are compiled into expression trees. This method does not evaluate immediately. Instead it returns an expression tree which can be further
        /// modified. During immediate evaluation, the expression tree is translated into a set of backend commands supported by its LINQ provider.
        /// In case of LINQ-to-SQL, the expression tree is translated into a set of SQL commands.
        /// </summary>
        /// <typeparam name="TSource">An underlying source type of a generic queryable collection.</typeparam>
        /// <param name="source">A queryable collection of type <see cref="T:System.Linq.IQueryable`1" /> that needs to be filtered.</param>
        /// <param name="rulesetXml">The XML containing one or more rules to be evaluated against source objects.
        /// The XML ruleset must validate against http://codeeffects.com/schemas/rule/4 schema.</param>
        /// <param name="parameters">An set of various parameters that determine how to build and evalute the rule. See <see cref="T:CodeEffects.Rule.Core.EvaluationParameters" /> for more information.</param>
        /// <returns>Returns an expression tree <see cref="T:System.Linq.IQueryable`1" /> for deferred or immediate evaluation. Immediate evaluation is handled by a LINQ provider.</returns>
        public static IQueryable<TSource> Filter<TSource>(this IQueryable<TSource> source, string rulesetXml, EvaluationParameters parameters)
        {
            Evaluator<TSource> evaluator = new Evaluator<TSource>(rulesetXml, parameters);
            Expression<Func<TSource, bool>> expression = evaluator.GetPredicateExpression(parameters.RuleId);
            LinqProviderType linqProviderType = parameters.LINQProviderType;
            if (linqProviderType == LinqProviderType.Default)
            {
                linqProviderType = RuleExtensions.GetProviderType(source);
            }
            if (linqProviderType == LinqProviderType.SQL || linqProviderType == LinqProviderType.Entities)
            {
                expression = (Expression<Func<TSource, bool>>)RuleExtensions.ReplaceIndexOfMethod(expression);
            }
            Vector.DelayIfDemo();
            evaluator.SuspendDemoDelay = true;
            return source.Where(expression);
        }

        /// <summary>
        /// Filters a collection of source objects by applying a business rule. The rulesetXml is loaded into <see cref="T:CodeEffects.Rule.Core.Evaluator`1" />
        /// and its rules are compiled into expression trees. This method does not evaluate immediately. Instead it returns an expression tree which can be further
        /// modified. During immediate evaluation, the expression tree is translated into a set of backend commands supported by its LINQ provider.
        /// In case of LINQ-to-SQL, the expression tree is translated into a set of SQL commands.
        /// </summary>
        /// <typeparam name="TSource">An underlying source type of a generic queryable collection.</typeparam>
        /// <param name="source">A queryable collection of type <see cref="T:System.Linq.IQueryable`1" /> that needs to be filtered.</param>
        /// <param name="rulesetXml">The XML containing one or more rules to be evaluated against source objects.
        /// The XML ruleset must validate against http://codeeffects.com/schemas/rule/4 schema.</param>
        /// <param name="ruleId">An optional id of a rule. If there are more than one rule in the ruleset, you may evaluate a specific
        /// rule by providing its id. <remarks>Not to be confused with external reusable rules referenced in rules inside of the rulesetXml.
        /// The rule with this id must be present as a top level rule inside the rulesetXml or an exception will be thrown.</remarks></param>
        /// <param name="getRule">The delegate to a method used to retrieve external rules by id (reusable rules referenced by other rules).
        /// If this parameter is null, the Evaluator will attempt to find any referenced rules inside the rulesetXml. If none are found
        /// an exception will be thrown.</param>
        /// <returns>Returns an expression tree <see cref="T:System.Linq.IQueryable`1" /> for deferred or immediate evaluation. Immediate evaluation is handled by a LINQ provider.</returns>
        public static IQueryable<TSource> Filter<TSource>(this IQueryable<TSource> source, string rulesetXml, string ruleId = null, GetRuleDelegate getRule = null)
        {
            EvaluationParameters parameters = new EvaluationParameters
            {
                RuleId = ruleId,
                RuleGetter = getRule,
                LINQProviderType = LinqProviderType.Default,
                PerformNullChecks = false
            };
            return source.Filter(rulesetXml, parameters);
        }

        /// <summary>
        /// Obsolete. This overload allows to evalute business rules by index, instead of id. Use other method instead.
        /// </summary>
        /// <typeparam name="TSource">An underlying source type of a generic queryable collection.</typeparam>
        /// <param name="source">A queryable collection of type <see cref="T:System.Linq.IQueryable`1" /> that needs to be filtered.</param>
        /// <param name="rulesetXml">The XML containing one or more rules to be evaluated against source objects.
        /// The XML ruleset must validate against http://codeeffects.com/schemas/rule/4 schema.</param>
        /// <param name="ruleIndex">A zero based index of a rule within the ruleset.</param>
        /// <param name="getRule">The delegate to a method used to retrieve external rules by id (reusable rules referenced by other rules).
        /// If this parameter is null, the Evaluator will attempt to find any referenced rules inside the rulesetXml. If none are found
        /// an exception will be thrown.</param>
        /// <returns>Returns an expression tree <see cref="T:System.Linq.IQueryable`1" /> for deferred or immediate evaluation. Immediate evaluation is handled by a LINQ provider.</returns>
        [Obsolete("Consider using a Filter overload with the ruleId parameter instead.")]
        public static IQueryable<TSource> Filter<TSource>(this IQueryable<TSource> source, string rulesetXml, int ruleIndex, GetRuleDelegate getRule = null)
        {
            XElement xElement = XElement.Parse(rulesetXml);
            if (source == null || xElement == null)
            {
                throw new ArgumentNullException();
            }
            XNamespace ns = xElement.GetDefaultNamespace();
            IEnumerable<XElement> source2 = from x in xElement.Element(ns + "codeeffects").Elements()
                                            where x.Name == ns + "rule"
                                            select x;
            if (ruleIndex >= source2.Count<XElement>() || ruleIndex < 0)
            {
                throw new IndexOutOfRangeException("Rule index is out of range.");
            }
            string text = (string)source2.ElementAt(ruleIndex).Attribute("id");
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException("Rule must have non-empty 'id' attribute.");
            }
            return source.Filter(rulesetXml, text, getRule);
        }

        /// <summary>
        /// Evaluates a rule from the ruleset against given source object. This extension method is just a shortcut for
        /// calling <see cref="M:CodeEffects.Rule.Core.Evaluator`1.Evaluate(`0,System.String)" /> method.
        /// <remarks>Use <see cref="T:CodeEffects.Rule.Core.Evaluator`1" /> directly if you plan on executing multiple Evaluate calls
        /// with the same set of rules. The rule evaluator compiles rules once per instantiating.</remarks>
        /// </summary>
        /// <typeparam name="TSource">An underlying source type of an object that will have rules evaluated against.</typeparam>
        /// <param name="source">An object that needs to be evaluated against a rule.</param>
        /// <param name="rulesetXml">A set of rules. If no rule id is provided, the first rule in the set will be executed.</param>
        /// <param name="ruleId">An id of a rule to be executed.</param>
        /// <param name="getRule">The delegate to a method used to retrieve external rules (rules referenced by other rules).
        /// If this parameter is null, the Evaluator will attempt to find any referenced rules inside the ruleset. If none are found
        /// an exception will be thrown.</param>
        /// <returns>True if rule evaluated successfully, False otherwise.</returns>
        public static bool Evaluate<TSource>(this TSource source, string rulesetXml, string ruleId = null, GetRuleDelegate getRule = null)
        {
            XElement xElement = XElement.Parse(rulesetXml);
            if (source == null || xElement == null)
            {
                throw new ArgumentNullException();
            }
            Evaluator<TSource> evaluator = new Evaluator<TSource>(rulesetXml, getRule, -1);
            Func<TSource, bool> predicate = evaluator.GetPredicate(ruleId);
            Vector.DelayIfDemo();
            return predicate(source);
        }

        /// <summary>
        /// Evaluates a rule from the ruleset against given source object. This extension method is just a shortcut for
        /// calling <see cref="M:CodeEffects.Rule.Core.Evaluator`1.Evaluate(`0,System.String)" /> method.
        /// <remarks>Use <see cref="T:CodeEffects.Rule.Core.Evaluator`1" /> directly if you plan on executing multiple Evaluate calls
        /// with the same set of rules. The rule evaluator compiles rules once per instantiating.</remarks>
        /// </summary>
        /// <typeparam name="TSource">An underlying source type of an object that will have rules evaluated against.</typeparam>
        /// <param name="source">An object that needs to be evaluated against a rule.</param>
        /// <param name="rulesetXml">A set of rules. The rule at the provided index will be executed.</param>
        /// <param name="ruleIndex">An index of a rule to be executed.</param>
        /// <param name="getRule">The delegate to a method used to retrieve external rules (rules referenced by other rules).
        /// If this parameter is null, the Evaluator will attempt to find any referenced rules inside the ruleset. If none are found
        /// an exception will be thrown.</param>
        /// <returns>True if rule evaluated successfully, False otherwise.</returns>
        public static bool Evaluate<TSource>(this TSource source, string rulesetXml, int ruleIndex, GetRuleDelegate getRule)
        {
            XElement xElement = XElement.Parse(rulesetXml);
            if (source == null || xElement == null)
            {
                throw new ArgumentNullException();
            }
            Evaluator<TSource> evaluator = new Evaluator<TSource>(rulesetXml, getRule, -1);
            Func<TSource, bool> predicate = evaluator.GetPredicate(ruleIndex);
            Vector.DelayIfDemo();
            return predicate(source);
        }

        /// <summary>
        /// This is a hack. It tries to determine whether given source has LINQ-to-SQL provider.
        /// It relies on the fact LINQ-to-SQL designer uses the DataQuery class which has hidden field "context" of type DataContext.
        /// This issue arises due to the fact that when filtering a LINQ-to-SQL generated collections, the source is of type DataQuery,
        /// which is an internal Microsoft class and there is no other way to verify the actual provider.
        /// </summary>
        /// <param name="source">An enumerable collection of type <see cref="T:System.Linq.IQueryable`1" /> for which a provider needs to be determined.</param>
        /// <returns>Returns the <see cref="T:CodeEffects.Rule.Core.LinqProviderType" /> enum.</returns>
        private static LinqProviderType GetProviderType(IQueryable source)
        {
            Type type = source.GetType();
            FieldInfo field = type.GetField("context", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null && field.FieldType.IsAssignableFrom(typeof(DataContext)))
            {
                DataContext dataContext = (DataContext)field.GetValue(source);
                if (dataContext.Mapping.ProviderType.IsAssignableFrom(typeof(SqlProvider)))
                {
                    return LinqProviderType.SQL;
                }
            }
            if (source.Provider.GetType().Name == "ObjectQueryProvider" || source.Provider.GetType().Name == "DbQueryProvider")
            {
                return LinqProviderType.Entities;
            }
            return LinqProviderType.Default;
        }

        internal static Expression ReplaceIndexOfMethod(Expression ruleLambdaExpression)
        {
            ConversionVisitor conversionVisitor = new ConversionVisitor();
            return conversionVisitor.Visit(ruleLambdaExpression);
        }
    }
}
