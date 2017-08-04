using ESPL.Rule.Core;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ESPL.Rule.Common
{
    internal class ExpressionBuilderBase
	{
		private enum CollectionTypeEnum
		{
			NotSupported,
			Queryable,
			Enumerable
		}

		private class ParameterComparer : IEqualityComparer<Type>
		{
			public bool Equals(Type x, Type y)
			{
				return x.IsAssignableFrom(y);
			}

			public int GetHashCode(Type obj)
			{
				return obj.GetHashCode();
			}
		}

		protected static readonly string[] mathOperators = new string[]
		{
			"add",
			"subtract",
			"multiply",
			"divide"
		};

		protected Type sourceType;

		protected ParameterExpression source;

		protected GetRuleInternalDelegate getRule;

		private int precision = -1;

		public int MaxIterations
		{
			get;
			set;
		}

		public bool PerformNullChecks
		{
			get;
			set;
		}

		public EvaluationScope EvaluationScope
		{
			get;
			set;
		}

		public bool ShortCircuit
		{
			get;
			set;
		}

		public int Precision
		{
			get
			{
				return this.precision;
			}
			set
			{
				this.precision = value;
			}
		}

		protected ExpressionBuilderBase(Type sourceType, GetRuleInternalDelegate getRule)
		{
			this.sourceType = sourceType;
			this.source = Expression.Parameter(sourceType, "x");
			this.getRule = getRule;
			this.PerformNullChecks = true;
		}

		protected Expression GetSafeExpressionBody(XElement rule, bool addSourceNullCheck)
		{
			Expression expression = this.Build(rule.Element(rule.GetDefaultNamespace() + "definition").Elements());
			Expression result;
			if (expression.NodeType == ExpressionType.Loop)
			{
				LoopExpression loopExpression = (LoopExpression)expression;
				result = loopExpression;
			}
			else if (expression.NodeType == ExpressionType.Conditional)
			{
				ConditionalExpression conditionalExpression = (ConditionalExpression)expression;
				ParameterExpression parameterExpression = Expression.Variable(typeof(bool), "result");
				result = Expression.Block(new ParameterExpression[]
				{
					parameterExpression
				}, new Expression[]
				{
					Expression.Assign(parameterExpression, conditionalExpression.Test),
					Expression.IfThenElse(parameterExpression, conditionalExpression.IfTrue, conditionalExpression.IfFalse),
					parameterExpression
				});
			}
			else if (addSourceNullCheck && !this.source.Type.IsValueType)
			{
				result = Expression.Condition(Expression.Equal(this.source, Expression.Constant(null)), Expression.Constant(false), expression);
			}
			else
			{
				result = expression;
			}
			return result;
		}

		/// <summary>
		/// A body of the rule used to be a method, a condition, an "or", an "and", or any other single element, so long it evaluates to a boolean.
		/// Now it also may be multiple "if" elements. When the EvaluationScope property is All, "if" statements are wrapped in the "AND", otherwise the "OR".
		/// If the ShortCircuit is false, all "if" stements will be evaluated even if the answer is already known.
		/// </summary>
		/// <param name="elements">A collection of a single valid statement or multiple 'if' statements</param>
		/// <returns>An expression tree representing the rule</returns>
		internal Expression Build(IEnumerable<XElement> elements)
		{
			int num = elements.Count<XElement>();
			if (num == 1)
			{
				return this.Build(elements.First<XElement>());
			}
			if (num == 0)
			{
				throw new InvalidRuleException(InvalidRuleException.ErrorIds.InvalidDefinitionStructure, new string[0]);
			}
			if (!elements.All((XElement x) => x.Name.LocalName == "if"))
			{
				throw new InvalidRuleException(InvalidRuleException.ErrorIds.InvalidNumberOfIfElements, new string[0]);
			}
			return this.BuildIfBlock(elements);
		}

		internal Expression Build(XElement element)
		{
			if (element == null)
			{
				return null;
			}
			string localName = element.Name.LocalName;
			string key;
			switch (key = localName)
			{
			case "and":
			case "or":
				return this.BuildMultiBooleanOperator(element, localName);
			case "not":
				return this.BuildNotOperator(element);
			case "condition":
				return this.BuildCondition(element);
			case "property":
				return this.BuildPropertyAccessor(element);
			case "value":
				return this.BuildValue(element);
			case "self":
				return this.source;
			case "expression":
				return this.BuildExpression(element.Elements().First<XElement>());
			case "method":
				return this.BuildMethod(element);
			case "rule":
				return this.BuildRule(element);
			case "if":
				return this.BuildIfRule(element);
			case "while":
				return this.BuildWhileRule(element);
			case "then":
			case "else":
				return this.BuildBlock(element);
			case "set":
				return this.BuildSetter(element);
			}
			return Expression.Empty();
		}

		private Expression BuildBlock(XElement element)
		{
			if (element.Elements().Count<XElement>() > 1)
			{
				List<Expression> list = new List<Expression>();
				foreach (XElement current in element.Elements())
				{
					list.Add(this.Build(current));
				}
				return Expression.Block(list);
			}
			return this.Build(element.Elements().FirstOrDefault<XElement>());
		}

		private Expression BuildSetter(XElement element)
		{
			if (element.Elements().Count<XElement>() != 2)
			{
				throw new ArgumentException("A Set element must have two child elements");
			}
			Expression expression = this.Build(element.Elements().First<XElement>());
			Expression expression2 = this.Build(element.Elements().Skip(1).First<XElement>());
			return Expression.Assign(expression, Expression.Convert(expression2, expression.Type));
		}

		private Expression BuildWhileRule(XElement element)
		{
			XNamespace defaultNamespace = element.GetDefaultNamespace();
			XElement xElement = element.Element(defaultNamespace + "clause");
			XElement element2 = element.Element(defaultNamespace + "then");
			Expression expression = this.Build(xElement.Elements().First<XElement>());
			Expression arg = this.Build(element2);
			LabelTarget labelTarget = Expression.Label();
			ParameterExpression parameterExpression = Expression.Variable(typeof(int));
			ParameterExpression parameterExpression2 = Expression.Variable(typeof(bool));
			Expression test;
			if (this.MaxIterations == -1)
			{
				test = expression;
			}
			else
			{
				test = Expression.AndAlso(Expression.LessThanOrEqual(parameterExpression, Expression.Constant(this.MaxIterations)), expression);
			}
			return Expression.Block(typeof(bool), new ParameterExpression[]
			{
				parameterExpression2,
				parameterExpression
			}, new Expression[]
			{
				Expression.Assign(parameterExpression2, Expression.Constant(false)),
				Expression.Assign(parameterExpression, Expression.Constant(0)),
				Expression.Loop(Expression.Block(new ParameterExpression[]
				{
					parameterExpression2,
					parameterExpression
				}, new Expression[]
				{
					Expression.Assign(parameterExpression, Expression.Increment(parameterExpression)),
					Expression.IfThenElse(test, Expression.Block(arg, Expression.Assign(parameterExpression2, Expression.Constant(true))), Expression.Break(labelTarget))
				}), labelTarget),
				parameterExpression2
			});
		}

		private Expression BuildIfBlock(IEnumerable<XElement> elements)
		{
			List<Expression> list = new List<Expression>();
			ParameterExpression parameterExpression = Expression.Variable(typeof(bool), "result");
			LabelTarget target = Expression.Label();
			bool flag = this.EvaluationScope == EvaluationScope.All;
			list.Add(Expression.Assign(parameterExpression, Expression.Constant(flag)));
			foreach (XElement current in elements)
			{
				ConditionalExpression conditionalExpression = (ConditionalExpression)this.Build(current);
				list.Add(Expression.IfThenElse(conditionalExpression.Test, flag ? conditionalExpression.IfTrue : (this.ShortCircuit ? Expression.Block(conditionalExpression.IfTrue, Expression.Assign(parameterExpression, Expression.Constant(true)), Expression.Goto(target)) : Expression.Block(conditionalExpression.IfTrue, Expression.Assign(parameterExpression, Expression.Constant(true)))), flag ? (this.ShortCircuit ? Expression.Block(conditionalExpression.IfFalse, Expression.Assign(parameterExpression, Expression.Constant(false)), Expression.Goto(target)) : Expression.Block(conditionalExpression.IfFalse, Expression.Assign(parameterExpression, Expression.Constant(false)))) : conditionalExpression.IfFalse));
			}
			list.Add(Expression.Label(target));
			list.Add(parameterExpression);
			return Expression.Block(new ParameterExpression[]
			{
				parameterExpression
			}, list.ToArray());
		}

		private Expression BuildIfRule(XElement element)
		{
			XNamespace defaultNamespace = element.GetDefaultNamespace();
			XElement xElement = element.Element(defaultNamespace + "clause");
			XElement element2 = element.Element(defaultNamespace + "then");
			XElement xElement2 = element.Element(defaultNamespace + "else");
			Expression test = this.Build(xElement.Elements().First<XElement>());
			Expression ifTrue = this.Build(element2);
			if (xElement2 != null)
			{
				Expression ifFalse = this.Build(xElement2);
				return Expression.IfThenElse(test, ifTrue, ifFalse);
			}
			return Expression.IfThen(test, ifTrue);
		}

		private Expression BuildMethod(XElement element)
		{
			XAttribute xAttribute = element.Attribute("name");
			XAttribute xAttribute2 = element.Attribute("instance");
			XAttribute xAttribute3 = element.Attribute("type");
			string value = xAttribute.Value;
			List<Expression> list = new List<Expression>();
			List<Type> list2 = new List<Type>();
			foreach (XElement current in element.Elements())
			{
				Expression expression = this.Build(current);
				list2.Add(expression.Type);
				list.Add(expression);
			}
			if (xAttribute2 != null && (string)xAttribute2 == "true")
			{
				if (list.Count == 0)
				{
					throw new ArgumentException("Method elements marked with instance='true' must have at least one child element to be the instance.");
				}
				Expression expression2 = list[0];
				Type type = expression2.Type;
				MethodInfo methodInfo = type.GetMethod(value, BindingFlags.Instance | BindingFlags.Public, null, list2.Skip(1).ToArray<Type>(), null);
				if (methodInfo == null)
				{
					methodInfo = this.FindBestMatchingMethod(value, type, list2.Skip(1).ToList<Type>(), BindingFlags.Instance | BindingFlags.Public);
				}
				if (methodInfo == null)
				{
					methodInfo = this.FindGenericCollectionOverload(value, type, list2.Skip(1).ToList<Type>(), BindingFlags.Instance | BindingFlags.Public);
				}
				if (methodInfo == null)
				{
					throw new MissingMethodException("Instance method '" + value + "' not found.");
				}
				return Expression.Call(expression2, methodInfo, list.Skip(1).ToArray<Expression>());
			}
			else
			{
				Type type;
				if (xAttribute3 != null)
				{
					type = Type.GetType(xAttribute3.Value);
				}
				else
				{
					type = this.sourceType;
				}
				MethodInfo methodInfo = type.GetMethod(value, list2.ToArray());
				if (methodInfo == null)
				{
					methodInfo = this.FindBestMatchingMethod(value, type, list2, BindingFlags.Public);
				}
				if (methodInfo == null)
				{
					methodInfo = this.FindGenericCollectionOverload(value, type, list2, BindingFlags.Public);
				}
				if (methodInfo == null)
				{
					throw new MissingMethodException("Method '" + value + "' not found.");
				}
				if (methodInfo.IsStatic)
				{
					return Expression.Call(methodInfo, list);
				}
				if (type.Equals(this.sourceType))
				{
					return Expression.Call(this.source, methodInfo, list);
				}
				return Expression.Call(Expression.New(type), methodInfo, list);
			}
		}

		private MethodInfo FindBestMatchingMethod(string methodName, Type type, List<Type> paramTypes, BindingFlags bindingFlags = BindingFlags.Public)
		{
			var orderedEnumerable = from x in type.GetMethods()
			where x.Name == methodName
			select new
			{
				method = x,
				parameters = x.GetParameters()
			} into x
			where x.parameters.Length == paramTypes.Count || (x.parameters.Length > paramTypes.Count && x.parameters[paramTypes.Count].IsOptional)
			orderby !x.method.IsGenericMethod, x.parameters.Length
			select x;
			foreach (var current in orderedEnumerable)
			{
				MethodInfo methodInfo = current.method;
				ParameterInfo[] parameters = current.parameters;
				BindingFlags bindingFlags2 = BindingFlags.Default;
				if (methodInfo.IsPublic)
				{
					bindingFlags2 |= BindingFlags.Public;
				}
				if (methodInfo.IsStatic)
				{
					bindingFlags2 |= BindingFlags.Static;
				}
				else
				{
					bindingFlags2 |= BindingFlags.Instance;
				}
				if (methodInfo.IsPrivate)
				{
					bindingFlags2 |= BindingFlags.NonPublic;
				}
				if ((bindingFlags2 & bindingFlags) == bindingFlags)
				{
					if (current.method.IsGenericMethod)
					{
						Type[] genericArguments = methodInfo.GetGenericArguments();
						if (genericArguments.Length > paramTypes.Count)
						{
							continue;
						}
						var enumerable = genericArguments.Select(delegate(Type argument)
						{
							ParameterInfo parameterInfo = parameters.FirstOrDefault((ParameterInfo x) => x.ParameterType == argument);
							Type type2 = (parameterInfo != null) ? paramTypes[parameterInfo.Position] : null;
							return new
							{
								argument = argument,
								type = type2
							};
						});
						if (enumerable.Any(x => x.type == null))
						{
							continue;
						}
						methodInfo = methodInfo.MakeGenericMethod((from x in enumerable
						select x.type).ToArray<Type>());
						parameters = methodInfo.GetParameters();
					}
					if (parameters.Take(paramTypes.Count).Select((ParameterInfo x, int i) => x.ParameterType.IsAssignableFrom(paramTypes[i])).All((bool x) => x))
					{
						return methodInfo;
					}
				}
			}
			return null;
		}

		private MethodInfo FindGenericCollectionOverload(string methodName, Type type, List<Type> paramTypes, BindingFlags bindingFlags = BindingFlags.Public)
		{
			Type[] genericCollectionItemTypes = (from x in paramTypes
			select new
			{
				param = x,
				enumerableType = x.GetInterface("IEnumerable`1")
			} into x
			where x.enumerableType != null
			select new
			{
				param = x.param,
				enumerableItemType = x.enumerableType.GetGenericArguments()[0]
			} into x
			where typeof(IEnumerable<>).MakeGenericType(new Type[]
			{
				x.enumerableItemType
			}).IsAssignableFrom(x.param)
			select x.enumerableItemType).Distinct<Type>().ToArray<Type>();
			List<MethodInfo> list = (from x in type.GetMethods()
			where x.Name == methodName && x.ContainsGenericParameters && x.GetGenericArguments().Length == genericCollectionItemTypes.Length
			select x).ToList<MethodInfo>();
			foreach (MethodInfo current in list)
			{
				BindingFlags bindingFlags2 = BindingFlags.Default;
				if (current.IsPublic)
				{
					bindingFlags2 |= BindingFlags.Public;
				}
				if (current.IsStatic)
				{
					bindingFlags2 |= BindingFlags.Static;
				}
				else
				{
					bindingFlags2 |= BindingFlags.Instance;
				}
				if (current.IsPrivate)
				{
					bindingFlags2 |= BindingFlags.NonPublic;
				}
				if ((bindingFlags2 & bindingFlags) == bindingFlags)
				{
					MethodInfo methodInfo = current.MakeGenericMethod(genericCollectionItemTypes);
					if ((from x in methodInfo.GetParameters()
					select x.ParameterType).SequenceEqual(paramTypes, new ExpressionBuilderBase.ParameterComparer()))
					{
						return methodInfo;
					}
				}
			}
			return null;
		}

		private Expression BuildExpression(XElement expressionBody)
		{
			string localName = expressionBody.Name.LocalName;
			Expression[] array = null;
			if (ExpressionBuilderBase.mathOperators.Contains(localName))
			{
				array = (from x in expressionBody.Elements()
				select this.BuildExpression(x)).ToArray<Expression>();
				ExpressionBuilderBase.CastToCommonType(array);
			}
			string a;
			if ((a = localName) != null)
			{
				if (a == "add")
				{
					return Expression.Add(array[0], array[1]);
				}
				if (a == "subtract")
				{
					return Expression.Subtract(array[0], array[1]);
				}
				if (a == "multiply")
				{
					return Expression.Multiply(array[0], array[1]);
				}
				if (a == "divide")
				{
					return Expression.Divide(array[0], array[1]);
				}
			}
			return this.Build(expressionBody);
		}

		private Expression BuildValue(XElement element)
		{
			XAttribute attribute = element.Attribute("type");
			string text = ((string)attribute) ?? "string";
			if (element.IsEmpty)
			{
				string key;
				switch (key = text)
				{
				case "bool":
					return Expression.Constant(false);
				case "time":
					return Expression.Constant(default(TimeSpan));
				case "date":
				case "datetime":
					return Expression.Constant(default(DateTime));
				case "integer":
					return Expression.Constant(0);
				case "numeric":
					return Expression.Constant(0m);
				case "double":
					return Expression.Constant(0.0);
				case "string":
					return Expression.Constant(null, typeof(string));
				}
				Type type = Type.GetType(text, true, true);
				if (ExpressionBuilderBase.IsGenericNullable(type))
				{
					return Expression.Constant(null, type);
				}
				return Expression.Constant(null, type);
			}
			else if (string.IsNullOrWhiteSpace(element.Value))
			{
				if (text == "string")
				{
					return Expression.Constant(element.Value);
				}
				throw new ArgumentException("Value elements of value types may not contain empty content. Consider empty element to get default values, i.e. <value type='int'/>");
			}
			else
			{
				string key2;
				switch (key2 = text)
				{
				case "bool":
					return Expression.Constant(bool.Parse(element.Value));
				case "time":
					return Expression.Constant(TimeSpan.Parse(element.Value, CultureInfo.InvariantCulture));
				case "date":
				case "datetime":
					return Expression.Constant(DateTime.Parse(element.Value, CultureInfo.InvariantCulture));
				case "integer":
					return Expression.Constant(int.Parse(element.Value, CultureInfo.InvariantCulture));
				case "numeric":
					return Expression.Constant(decimal.Parse(element.Value, CultureInfo.InvariantCulture));
				case "double":
					return Expression.Constant(double.Parse(element.Value, CultureInfo.InvariantCulture));
				case "string":
					return Expression.Constant(element.Value);
				}
				Type type2 = Type.GetType(text, true, true);
				if (type2.IsEnum)
				{
					return Expression.Constant(Enum.Parse(type2, element.Value), type2);
				}
				if (ExpressionBuilderBase.IsGenericNullable(type2))
				{
					Type underlyingType = Nullable.GetUnderlyingType(type2);
					MethodInfo method = underlyingType.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, null, new Type[]
					{
						typeof(string),
						typeof(IFormatProvider)
					}, null);
					if (method != null)
					{
						try
						{
							Expression result = Expression.Convert(Expression.Constant(method.Invoke(null, new object[]
							{
								element.Value,
								CultureInfo.InvariantCulture
							})), typeof(Nullable<>).MakeGenericType(new Type[]
							{
								underlyingType
							}));
							return result;
						}
						catch
						{
						}
					}
					method = underlyingType.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, null, new Type[]
					{
						typeof(string)
					}, null);
					if (!(method != null))
					{
						//TODO: goto IL_546;
					}
					try
					{
						Expression result = Expression.Convert(Expression.Constant(method.Invoke(null, new object[]
						{
							element.Value
						})), typeof(Nullable<>).MakeGenericType(new Type[]
						{
							underlyingType
						}));
						return result;
					}
					catch
					{
						//TODO: goto IL_546;
					}
				}
				MethodInfo method2 = type2.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, null, new Type[]
				{
					typeof(string),
					typeof(IFormatProvider)
				}, null);
				if (method2 != null)
				{
					try
					{
						Expression result = Expression.Constant(method2.Invoke(null, new object[]
						{
							element.Value,
							CultureInfo.InvariantCulture
						}));
						return result;
					}
					catch
					{
					}
				}
				method2 = type2.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, null, new Type[]
				{
					typeof(string)
				}, null);
				if (method2 != null)
				{
					try
					{
						Expression result = Expression.Constant(method2.Invoke(null, new object[]
						{
							element.Value
						}));
						return result;
					}
					catch
					{
					}
				}
				try
				{
					IL_546:
					XmlSerializer xmlSerializer = new XmlSerializer(type2, element.GetDefaultNamespace().NamespaceName);
					using (StringReader stringReader = new StringReader(element.FirstNode.ToString(SaveOptions.DisableFormatting)))
					{
						Expression result = Expression.Constant(xmlSerializer.Deserialize(stringReader), type2);
						return result;
					}
				}
				catch
				{
				}
				try
				{
					JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
					Expression result = Expression.Constant(javaScriptSerializer.Deserialize(element.Value, type2), type2);
					return result;
				}
				catch
				{
				}
				throw new ArgumentException("All attempts to deserialize have failed: Parse, Xml, and JSON.");
			}
		}

		private Expression BuildExistsExpression(XElement existsElement)
		{
			string text = (string)existsElement.Attribute("itemType");
			Type type = string.IsNullOrWhiteSpace(text) ? null : Type.GetType(text);
			Expression expression = this.Build(existsElement.Elements().First<XElement>());
			Type @interface = expression.Type.GetInterface("IEnumerable`1");
			if (@interface != null)
			{
				if (type != null)
				{
					expression = Expression.TypeAs(expression, typeof(IEnumerable<>).MakeGenericType(new Type[]
					{
						type
					}));
				}
				else
				{
					type = @interface.GetGenericArguments()[0];
				}
			}
			else
			{
				Type interface2 = expression.Type.GetInterface("IEnumerable");
				if (interface2 != null)
				{
					expression = Expression.Call(typeof(Enumerable), "Cast", new Type[]
					{
						type
					}, new Expression[]
					{
						expression
					});
				}
			}
			ExpressionBuilder expressionBuilder = new ExpressionBuilder(type, this.getRule);
			Expression body = expressionBuilder.Build(existsElement.Elements(existsElement.GetDefaultNamespace() + "where").Elements<XElement>().First<XElement>());
			LambdaExpression lambdaExpression = Expression.Lambda(body, new ParameterExpression[]
			{
				expressionBuilder.source
			});
			return Expression.Call(typeof(Enumerable), "Any", new Type[]
			{
				type
			}, new Expression[]
			{
				expression,
				lambdaExpression
			});
		}

		private Expression BuildCondition(XElement conditionElement)
		{
			XAttribute xAttribute = conditionElement.Attribute("type");
			string text = (xAttribute != null) ? xAttribute.Value : null;
			XElement[] array = conditionElement.Elements().ToArray<XElement>();
			Expression[] array2 = new Expression[array.Length];
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i] = this.Build(array[i]);
			}
			List<Expression> list = new List<Expression>();
			for (int j = 0; j < array2.Length; j++)
			{
				list.Add(array2[j]);
			}
			Expression expression = this.BuildPropertyNullChecks(list.ToArray());
			if (!typeof(IEnumerable).IsAssignableFrom(array2[0].Type) && !string.Equals(text, "isNull", StringComparison.OrdinalIgnoreCase) && !string.Equals(text, "isNotNull", StringComparison.OrdinalIgnoreCase) && array2.Length > 0)
			{
				ExpressionBuilderBase.CastToCommonType(array2);
			}
			if (this.Precision != -1)
			{
				for (int k = 0; k < array2.Length; k++)
				{
					if (array2[k].Type == typeof(double) || array2[k].Type == typeof(decimal))
					{
						array2[k] = Expression.Call(typeof(Math).GetMethod("Round", new Type[]
						{
							array2[k].Type,
							typeof(int)
						}), array2[k], Expression.Constant(this.Precision));
					}
					else if (array2[k].Type == typeof(float))
					{
						array2[k] = Expression.Call(typeof(Math).GetMethod("Round", new Type[]
						{
							array2[k].Type,
							typeof(int)
						}), Expression.Convert(array2[k], typeof(double)), Expression.Constant(this.Precision));
					}
				}
			}
			string key;
			if ((key = text) != null)
			{
                //TODO: //if (<PrivateImplementationDetails>{C6EA5F6E-C064-4F46-8F27-151D6168C23D}.$$method0x6000295-1 == null)
                //{
                //    <PrivateImplementationDetails>{C6EA5F6E-C064-4F46-8F27-151D6168C23D}.$$method0x6000295-1 = new Dictionary<string, int>(15)
                //    {
                //        {
                //            "equal",
                //            0
                //        },
                //        {
                //            "notEqual",
                //            1
                //        },
                //        {
                //            "less",
                //            2
                //        },
                //        {
                //            "lessOrEqual",
                //            3
                //        },
                //        {
                //            "greater",
                //            4
                //        },
                //        {
                //            "greaterOrEqual",
                //            5
                //        },
                //        {
                //            "isNull",
                //            6
                //        },
                //        {
                //            "isNotNull",
                //            7
                //        },
                //        {
                //            "startsWith",
                //            8
                //        },
                //        {
                //            "doesNotStartWith",
                //            9
                //        },
                //        {
                //            "endsWith",
                //            10
                //        },
                //        {
                //            "doesNotEndWith",
                //            11
                //        },
                //        {
                //            "contains",
                //            12
                //        },
                //        {
                //            "doesNotContain",
                //            13
                //        },
                //        {
                //            "between",
                //            14
                //        }
                //    };
                //}
				int num;
//                if (<PrivateImplementationDetails>{C6EA5F6E-C064-4F46-8F27-151D6168C23D}.$$method0x6000295-1.TryGetValue(key, out num))
//                {
//                    Expression expression2;
//#region switch(num)
		 
//                    switch (num)
//                    {
//                    case 0:
//                        expression2 = this.BuildEqualsMethodExpression(conditionElement, array2);
//                        break;
//                    case 1:
//                        expression2 = Expression.Not(this.BuildEqualsMethodExpression(conditionElement, array2));
//                        break;
//                    case 2:
//                    {
//                        Tuple<Expression, Expression> tuple = this.BuildBinaryComparisonMethodExpression(conditionElement, array2);
//                        expression2 = Expression.LessThan(tuple.Item1, tuple.Item2);
//                        break;
//                    }
//                    case 3:
//                    {
//                        Tuple<Expression, Expression> tuple = this.BuildBinaryComparisonMethodExpression(conditionElement, array2);
//                        expression2 = Expression.LessThanOrEqual(tuple.Item1, tuple.Item2);
//                        break;
//                    }
//                    case 4:
//                    {
//                        Tuple<Expression, Expression> tuple = this.BuildBinaryComparisonMethodExpression(conditionElement, array2);
//                        expression2 = Expression.GreaterThan(tuple.Item1, tuple.Item2);
//                        break;
//                    }
//                    case 5:
//                    {
//                        Tuple<Expression, Expression> tuple = this.BuildBinaryComparisonMethodExpression(conditionElement, array2);
//                        expression2 = Expression.GreaterThanOrEqual(tuple.Item1, tuple.Item2);
//                        break;
//                    }
//                    case 6:
//                        expression2 = this.BuildIsEmptyExpression(conditionElement, array2);
//                        break;
//                    case 7:
//                        expression2 = Expression.Not(this.BuildIsEmptyExpression(conditionElement, array2));
//                        break;
//                    case 8:
//                        expression2 = this.BuildStartsOrEndsWithExpression("StartsWith", conditionElement, array2);
//                        break;
//                    case 9:
//                        expression2 = Expression.Not(this.BuildStartsOrEndsWithExpression("StartsWith", conditionElement, array2));
//                        break;
//                    case 10:
//                        expression2 = this.BuildStartsOrEndsWithExpression("EndsWith", conditionElement, array2);
//                        break;
//                    case 11:
//                        expression2 = Expression.Not(this.BuildStartsOrEndsWithExpression("EndsWith", conditionElement, array2));
//                        break;
//                    case 12:
//                        expression2 = this.BuildContainsExpression(conditionElement, array2);
//                        break;
//                    case 13:
//                        expression2 = Expression.Not(this.BuildContainsExpression(conditionElement, array2));
//                        break;
//                    case 14:
//                        expression2 = Expression.AndAlso(Expression.GreaterThanOrEqual(array2[0], array2[1]), Expression.LessThan(array2[0], array2[2]));
//                        break;
//                    default:
//                        goto IL_496;
//                    }
//    #endregion

//                    if (expression != null && this.PerformNullChecks)
//                    {
//                        expression2 = Expression.AndAlso(expression, expression2);
//                    }
//                    return expression2;
//                }
			}
			IL_496:
			throw new ArgumentException(string.Format("Condition elements do not support this operator type: '{0}'", text));
		}

		private Expression BuildIsEmptyExpression(XElement conditionElement, Expression[] expressions)
		{
			Expression expression = Expression.Equal(expressions[0], Expression.Constant(null));
			if (expressions[0].Type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(expressions[0].Type))
			{
				Expression expression2 = expressions[0];
				Type type = typeof(object);
				if (!expressions[0].Type.IsGenericType && !expressions[0].Type.IsArray)
				{
					conditionElement.Elements().First<XElement>();
					MethodInfo extensionMethod = this.GetExtensionMethod(typeof(Enumerable), "Cast.*IEnumerable");
					MethodCallExpression methodCallExpression = Expression.Call(null, extensionMethod.MakeGenericMethod(new Type[]
					{
						type
					}), new Expression[]
					{
						expression2
					});
					expression2 = methodCallExpression;
				}
				else if (expressions[0].Type.IsArray)
				{
					type = expressions[0].Type.GetElementType();
				}
				else
				{
					type = expressions[0].Type.GetGenericArguments()[0];
				}
				MethodInfo extensionMethod2 = this.GetExtensionMethod(typeof(Enumerable), "Count.*IEnumerable`1\\[TSource\\]\\)");
				MethodCallExpression left = Expression.Call(null, extensionMethod2.MakeGenericMethod(new Type[]
				{
					type
				}), new Expression[]
				{
					expression2
				});
				expression = Expression.OrElse(expression, Expression.Equal(left, Expression.Constant(0)));
			}
			return expression;
		}

		private Expression BuildStartsOrEndsWithExpression(string methodName, XElement conditionElement, Expression[] expressions)
		{
			if (!string.Equals(methodName, "startsWith", StringComparison.OrdinalIgnoreCase) && !string.Equals(methodName, "endsWith", StringComparison.OrdinalIgnoreCase) && !string.Equals(methodName, "doesNotStartWith", StringComparison.OrdinalIgnoreCase) && !string.Equals(methodName, "doesNotEndWith", StringComparison.OrdinalIgnoreCase))
			{
				throw new Exception(string.Format("Method {0} is not supported as condition operator.", methodName));
			}
			Expression expression;
			if (expressions[0].Type == typeof(string))
			{
				for (int i = 0; i < expressions.Length; i++)
				{
					if (expressions[i].NodeType == ExpressionType.Constant && ((ConstantExpression)expressions[i]).Value == null)
					{
						return Expression.Constant(false);
					}
				}
				XAttribute attribute = conditionElement.Attribute("stringComparison");
				StringComparison stringComparison;
				if (!Enum.TryParse<StringComparison>((string)attribute, out stringComparison))
				{
					stringComparison = StringComparison.Ordinal;
				}
				expression = Expression.Call(expressions[0], methodName, null, new Expression[]
				{
					expressions[1],
					Expression.Constant(stringComparison)
				});
				if (this.PerformNullChecks)
				{
					expression = this.AddLocalNullChecks(expression, expressions);
				}
			}
			else
			{
				if (typeof(IEnumerable).IsAssignableFrom(expressions[0].Type))
				{
					Expression expression2 = expressions[0];
					Type type;
					if (!expressions[0].Type.IsGenericType && !expressions[0].Type.IsArray)
					{
						XElement xElement = conditionElement.Elements().First<XElement>();
						if (xElement.Attribute("itemType") != null)
						{
							type = Type.GetType((string)xElement.Attribute("itemType"));
						}
						else
						{
							type = expressions[1].Type;
						}
						MethodInfo extensionMethod = this.GetExtensionMethod(typeof(Enumerable), "Cast.*IEnumerable");
						MethodCallExpression methodCallExpression = Expression.Call(null, extensionMethod.MakeGenericMethod(new Type[]
						{
							type
						}), new Expression[]
						{
							expression2
						});
						expression2 = methodCallExpression;
					}
					else if (expressions[0].Type.IsArray)
					{
						type = expressions[0].Type.GetElementType();
					}
					else
					{
						type = expressions[0].Type.GetGenericArguments()[0];
					}
					MethodInfo extensionMethod2;
					if (string.Equals(methodName, "startsWith", StringComparison.OrdinalIgnoreCase) || string.Equals(methodName, "doesNotStartWith", StringComparison.OrdinalIgnoreCase))
					{
						extensionMethod2 = this.GetExtensionMethod(typeof(Enumerable), "First.*IEnumerable`1\\[TSource\\]\\)");
					}
					else
					{
						extensionMethod2 = this.GetExtensionMethod(typeof(Enumerable), "Last.*IEnumerable`1\\[TSource\\]\\)");
					}
					MethodCallExpression left = Expression.Call(null, extensionMethod2.MakeGenericMethod(new Type[]
					{
						type
					}), new Expression[]
					{
						expression2
					});
					MethodInfo extensionMethod3 = this.GetExtensionMethod(typeof(Enumerable), "Any.*IEnumerable`1\\[TSource\\]\\)");
					MethodCallExpression left2 = Expression.Call(null, extensionMethod3.MakeGenericMethod(new Type[]
					{
						type
					}), new Expression[]
					{
						expression2
					});
					expression = Expression.Equal(left, expressions[1]);
					expression = Expression.AndAlso(left2, expression);
				}
				else
				{
					MethodInfo method = expressions[0].Type.GetMethod(methodName);
					if (!(method != null))
					{
						throw new Exception(string.Format("Type {0} does not support {1}", expressions[0].Type.FullName, methodName));
					}
					expression = Expression.Call(expressions[0], methodName, null, new Expression[]
					{
						expressions[1]
					});
				}
				expression = Expression.AndAlso(Expression.NotEqual(expressions[0], Expression.Constant(null)), expression);
			}
			return expression;
		}

		private Expression BuildContainsExpression(XElement conditionElement, Expression[] expressions)
		{
			Expression expression;
			if (expressions[0].Type == typeof(string))
			{
				for (int i = 0; i < expressions.Length; i++)
				{
					if (expressions[i].NodeType == ExpressionType.Constant && ((ConstantExpression)expressions[i]).Value == null)
					{
						return Expression.Constant(false);
					}
				}
				XAttribute attribute = conditionElement.Attribute("stringComparison");
				StringComparison stringComparison;
				if (!Enum.TryParse<StringComparison>((string)attribute, out stringComparison))
				{
					stringComparison = StringComparison.Ordinal;
				}
				expression = Expression.NotEqual(Expression.Call(expressions[0], "IndexOf", null, new Expression[]
				{
					expressions[1],
					Expression.Constant(stringComparison)
				}), Expression.Constant(-1));
				if (this.PerformNullChecks)
				{
					expression = this.AddLocalNullChecks(expression, expressions);
				}
			}
			else
			{
				if (typeof(IEnumerable).IsAssignableFrom(expressions[0].Type))
				{
					Expression expression2 = expressions[0];
					Type type;
					if (!expressions[0].Type.IsGenericType && !expressions[0].Type.IsArray)
					{
						XElement xElement = conditionElement.Elements().First<XElement>();
						if (xElement.Attribute("itemType") != null)
						{
							type = Type.GetType((string)xElement.Attribute("itemType"));
						}
						else
						{
							type = expressions[1].Type;
						}
						MethodInfo extensionMethod = this.GetExtensionMethod(typeof(Enumerable), "Cast.*IEnumerable");
						MethodCallExpression methodCallExpression = Expression.Call(null, extensionMethod.MakeGenericMethod(new Type[]
						{
							type
						}), new Expression[]
						{
							expression2
						});
						expression2 = methodCallExpression;
					}
					else if (expressions[0].Type.IsArray)
					{
						type = expressions[0].Type.GetElementType();
					}
					else
					{
						type = expressions[0].Type.GetGenericArguments()[0];
					}
					MethodInfo extensionMethod2 = this.GetExtensionMethod(typeof(Enumerable), "Contains.*IEnumerable`1\\[TSource\\].*TSource\\)");
					MethodCallExpression methodCallExpression2 = Expression.Call(null, extensionMethod2.MakeGenericMethod(new Type[]
					{
						type
					}), new Expression[]
					{
						expression2,
						expressions[1]
					});
					expression = methodCallExpression2;
				}
				else
				{
					MethodInfo method = expressions[0].Type.GetMethod("Contains");
					if (!(method != null))
					{
						throw new Exception(string.Format("Type {0} does not support Contains", expressions[0].Type.FullName));
					}
					expression = Expression.Call(expressions[0], "Contains", null, new Expression[]
					{
						expressions[1]
					});
				}
				expression = Expression.AndAlso(Expression.NotEqual(expressions[0], Expression.Constant(null)), expression);
			}
			return expression;
		}

		private Expression AddLocalNullChecks(Expression condition, Expression[] expressions)
		{
			if (expressions[0].NodeType != ExpressionType.Constant)
			{
				if (expressions[1].NodeType != ExpressionType.Constant)
				{
					condition = Expression.AndAlso(Expression.AndAlso(Expression.NotEqual(expressions[0], Expression.Constant(null)), Expression.NotEqual(expressions[1], Expression.Constant(null))), condition);
				}
				else
				{
					condition = Expression.AndAlso(Expression.NotEqual(expressions[0], Expression.Constant(null)), condition);
				}
			}
			else if (expressions[1].NodeType != ExpressionType.Constant)
			{
				condition = Expression.AndAlso(Expression.NotEqual(expressions[1], Expression.Constant(null)), condition);
			}
			return condition;
		}

		private Expression BuildEqualsMethodExpression(XElement conditionElement, Expression[] expressions)
		{
			Expression result;
			if (expressions[0].Type != typeof(string))
			{
				result = Expression.Equal(expressions[0], expressions[1]);
			}
			else
			{
				XAttribute attribute = conditionElement.Attribute("stringComparison");
				StringComparison stringComparison;
				if (!Enum.TryParse<StringComparison>((string)attribute, out stringComparison))
				{
					stringComparison = StringComparison.Ordinal;
				}
				result = Expression.Call(typeof(string), "Equals", null, new Expression[]
				{
					expressions[0],
					expressions[1],
					Expression.Constant(stringComparison)
				});
			}
			return result;
		}

		private Tuple<Expression, Expression> BuildBinaryComparisonMethodExpression(XElement conditionElement, Expression[] expressions)
		{
			if (expressions[0].Type != typeof(string))
			{
				return new Tuple<Expression, Expression>(expressions[0], expressions[1]);
			}
			XAttribute attribute = conditionElement.Attribute("stringComparison");
			StringComparison stringComparison;
			if (!Enum.TryParse<StringComparison>((string)attribute, out stringComparison))
			{
				stringComparison = StringComparison.Ordinal;
			}
			return new Tuple<Expression, Expression>(Expression.Call(typeof(string), "Compare", null, new Expression[]
			{
				expressions[0],
				expressions[1],
				Expression.Constant(stringComparison)
			}), Expression.Constant(0));
		}

		private Expression BuildStringMethodExpression(string methodName, XElement conditionElement, Expression[] expressions)
		{
			Expression expression;
			if (expressions[0].Type != typeof(string))
			{
				expression = Expression.Call(expressions[0], methodName, null, new Expression[]
				{
					expressions[1]
				});
			}
			else
			{
				for (int i = 0; i < expressions.Length; i++)
				{
					if (expressions[i].NodeType == ExpressionType.Constant && ((ConstantExpression)expressions[i]).Value == null)
					{
						return Expression.Constant(false);
					}
				}
				XAttribute attribute = conditionElement.Attribute("stringComparison");
				StringComparison stringComparison;
				if (!Enum.TryParse<StringComparison>((string)attribute, out stringComparison))
				{
					stringComparison = StringComparison.Ordinal;
				}
				expression = Expression.Call(expressions[0], methodName, null, new Expression[]
				{
					expressions[1],
					Expression.Constant(stringComparison)
				});
				if (this.PerformNullChecks)
				{
					expression = this.AddLocalNullChecks(expression, expressions);
				}
			}
			return expression;
		}

		private static int NumericTypeRank(Type type)
		{
			if (type == typeof(int) || type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort) || type == typeof(char))
			{
				return 0;
			}
			if (type == typeof(uint) || type == typeof(long))
			{
				return 1;
			}
			if (type == typeof(decimal))
			{
				return 2;
			}
			if (type == typeof(float))
			{
				return 3;
			}
			if (type == typeof(double))
			{
				return 4;
			}
			return -1;
		}

		private static void CastToCommonType(Expression[] expressions)
		{
            //TODO: //Type type = expressions[0].Type.GetNonNullableType();
            //int num = ExpressionBuilderBase.NumericTypeRank(type);
            //if (num > ExpressionBuilderBase.NumericTypeRank(typeof(byte)))
            //{
            //    type = typeof(byte);
            //    num = ExpressionBuilderBase.NumericTypeRank(type);
            //}
            //bool flag = ExpressionBuilderBase.IsGenericNullable(type);
            //for (int i = 0; i < expressions.Length; i++)
            //{
            //    Type type2 = expressions[i].Type;
            //    if (ExpressionBuilderBase.IsGenericNullable(type2))
            //    {
            //        type2 = type2.GetNonNullableType();
            //        flag = true;
            //    }
            //    if (type2 == typeof(byte) || type2 == typeof(sbyte) || type2 == typeof(short) || type2 == typeof(ushort) || type2 == typeof(char))
            //    {
            //        type2 = typeof(int);
            //    }
            //    else if (type2 == typeof(uint))
            //    {
            //        type2 = typeof(long);
            //    }
            //    int num2 = ExpressionBuilderBase.NumericTypeRank(type2);
            //    if ((num2 >= 0 && num < num2) || (num2 < 0 && TypeUtils.IsImplicitlyConvertible(type, type2)))
            //    {
            //        type = type2;
            //        num = num2;
            //    }
            //}
            //for (int j = 0; j < expressions.Length; j++)
            //{
            //    Type type3 = expressions[j].Type;
            //    if (flag)
            //    {
            //        if (!ExpressionBuilderBase.IsGenericNullable(type3) || type3.GetNonNullableType() != type)
            //        {
            //            expressions[j] = Expression.Convert(expressions[j], typeof(Nullable<>).MakeGenericType(new Type[]
            //            {
            //                type
            //            }));
            //        }
            //    }
            //    else if (type3 != type)
            //    {
            //        expressions[j] = Expression.Convert(expressions[j], type);
            //    }
            //}
		}

		public static bool IsGenericNullable(Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}

		private Expression BuildPropertyNullChecks(params Expression[] expressions)
		{
			Expression expression = null;
			Stack<Expression> stack = new Stack<Expression>();
			List<string> list = new List<string>();
			for (int i = 0; i < expressions.Length; i++)
			{
				if (expressions[i].NodeType == ExpressionType.MemberAccess)
				{
					Expression expression2 = ((MemberExpression)expressions[i]).Expression;
					while (expression2.NodeType == ExpressionType.MemberAccess)
					{
						stack.Push(expression2);
						expression2 = ((MemberExpression)expression2).Expression;
					}
					while (stack.Count > 0)
					{
						expression2 = stack.Pop();
						if (!list.Contains(expression2.ToString()))
						{
							list.Add(expression2.ToString());
							if (expression == null)
							{
								expression = Expression.NotEqual(expression2, Expression.Constant(null));
							}
							else
							{
								expression = Expression.AndAlso(expression, Expression.NotEqual(expression2, Expression.Constant(null)));
							}
						}
					}
				}
			}
			return expression;
		}

		private Expression GetMemberExpression(Expression source, string name)
		{
			Expression result = null;
			if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(source.Type))
			{
				CallSiteBinder member = Microsoft.CSharp.RuntimeBinder.Binder.GetMember(CSharpBinderFlags.None, name, source.Type, new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				});
				result = Expression.Dynamic(member, typeof(object), source);
				return result;
			}
			try
			{
				result = Expression.PropertyOrField(source, name);
			}
			catch (AmbiguousMatchException)
			{
				MemberInfo[] member2 = source.Type.GetMember(name);
				result = Expression.MakeMemberAccess(source, member2[0]);
			}
			return result;
		}

		private Expression BuildPropertyAccessor(XElement element)
		{
			if (string.IsNullOrWhiteSpace((string)element.Attribute("name")))
			{
				throw new ArgumentException("A property element must have a non-empty name attribute");
			}
			string[] array = element.Attribute("name").Value.Split(new char[]
			{
				'.'
			});
			Expression expression = this.GetMemberExpression(this.source, array[0]);
			for (int i = 1; i < array.Length; i++)
			{
				expression = this.GetMemberExpression(expression, array[i]);
			}
			if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(this.source.Type))
			{
				CallSiteBinder binder = Microsoft.CSharp.RuntimeBinder.Binder.Convert(CSharpBinderFlags.None, expression.Type, this.source.Type);
				expression = Expression.Dynamic(binder, expression.Type, expression);
			}
			return expression;
		}

		private Expression BuildMultiBooleanOperator(XElement element, string elementName)
		{
			IEnumerable<Expression> enumerable = from x in element.Elements()
			select this.Build(x);
			Expression[] array = enumerable.ToArray<Expression>();
			if (array.Length < 1)
			{
				throw new ArgumentException("A multi-boolean operator (AND, OR, etc.) must have at least one child element.");
			}
			Expression expression = array[0];
			for (int i = 1; i < array.Length; i++)
			{
				if (elementName == "and")
				{
					expression = Expression.AndAlso(expression, array[i]);
				}
				else
				{
					expression = Expression.OrElse(expression, array[i]);
				}
			}
			return expression;
		}

		private XElement LoadRule(XElement element, string id)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element", "Cannot do anything when xml is empty.");
			}
			if (string.IsNullOrWhiteSpace(id))
			{
				throw new ArgumentNullException("id", "Missing rule id.");
			}
			XNamespace defaultNamespace = element.GetDefaultNamespace();
			XElement xElement = (from x in element.AncestorsAndSelf(defaultNamespace + "codeeffects").Elements(defaultNamespace + "rule")
			where (string)x.Attribute("id") == id
			select x).FirstOrDefault<XElement>();
			if (xElement == null && this.getRule != null)
			{
				xElement = this.getRule(id);
				if (xElement != null)
				{
					defaultNamespace = xElement.GetDefaultNamespace();
					if (xElement.Name == defaultNamespace + "codeeffects")
					{
						xElement = (from x in xElement.Elements(defaultNamespace + "rule")
						where (string)x.Attribute("id") == id
						select x).FirstOrDefault<XElement>();
					}
					else if (xElement.Name != defaultNamespace + "rule")
					{
						throw new Exception("Invalid rule xml. Expected either the <rule> node or the <codeeffects>, received <" + xElement.Name + "> instead.");
					}
				}
			}
			if (xElement == null)
			{
				throw new InvalidRuleException(InvalidRuleException.ErrorIds.RuleNotFound, new string[]
				{
					id
				});
			}
			return xElement;
		}

		private Expression BuildRule(XElement element)
		{
			XNamespace defaultNamespace = element.GetDefaultNamespace();
			string id = (string)element.Attribute("id");
			XElement xElement = this.LoadRule(element, id);
			XElement xElement2 = element.Elements().FirstOrDefault<XElement>();
			Expression expression = null;
			if (xElement2 != null)
			{
				expression = this.Build(xElement2);
			}
			if (expression != null)
			{
				Type type = Type.GetType((string)xElement.Attribute("type"));
				ParameterExpression parameterExpression = this.source;
				this.source = Expression.Parameter(type, "x");
				this.sourceType = type;
				bool performNullChecks = this.PerformNullChecks;
				if (typeof(IQueryable).IsAssignableFrom(expression.Type))
				{
					this.PerformNullChecks = false;
				}
				Expression safeExpressionBody = this.GetSafeExpressionBody(xElement, false);
				this.PerformNullChecks = performNullChecks;
				LambdaExpression ruleLambdaExpression = Expression.Lambda(safeExpressionBody, new ParameterExpression[]
				{
					this.source
				});
				Expression result;
				if (typeof(IQueryable).IsAssignableFrom(expression.Type) || typeof(IEnumerable).IsAssignableFrom(expression.Type))
				{
					result = this.BuildCollectionOperator((string)element.Attribute("operator"), expression, ruleLambdaExpression, type);
				}
				else
				{
					result = safeExpressionBody;
				}
				this.source = parameterExpression;
				this.sourceType = parameterExpression.Type;
				return result;
			}
			defaultNamespace = xElement.GetDefaultNamespace();
			xElement = xElement.Elements(defaultNamespace + "definition").Elements<XElement>().FirstOrDefault<XElement>();
			return this.Build(xElement);
		}

		private Expression BuildCollectionOperator(string @operator, Expression context, LambdaExpression ruleLambdaExpression, Type itemType)
		{
			Type type = context.Type;
			Expression left = Expression.NotEqual(context, Expression.Constant(null));
			ExpressionBuilderBase.CollectionTypeEnum collectionTypeEnum;
			MethodInfo extensionMethod;
			if (typeof(IQueryable).IsAssignableFrom(type))
			{
				collectionTypeEnum = ExpressionBuilderBase.CollectionTypeEnum.Queryable;
				extensionMethod = this.GetExtensionMethod(typeof(Queryable), "Any.*IQueryable`1\\[TSource\\]\\)");
				ruleLambdaExpression = (LambdaExpression)RuleExtensions.ReplaceIndexOfMethod(ruleLambdaExpression);
			}
			else
			{
				if (!typeof(IEnumerable).IsAssignableFrom(type))
				{
					throw new Exception(string.Format("Operator {0} is only supported for collections that implement IEnumerable, IEnumerable<T>, IQueryable, and IQueryable<T>. Type '{1}' is not supported.", @operator, type.AssemblyQualifiedName));
				}
				collectionTypeEnum = ExpressionBuilderBase.CollectionTypeEnum.Enumerable;
				extensionMethod = this.GetExtensionMethod(typeof(Enumerable), "Any.*IEnumerable`1\\[TSource\\]\\)");
			}
			if (@operator != null && (@operator == "exists" || @operator == "doesNotExist"))
			{
				string pattern = "Any.*IQueryable`1.*Expression`1.*Func`2";
				string pattern2 = "Any.*IEnumerable`1.*Func`2";
				Expression[] array;
				if (collectionTypeEnum == ExpressionBuilderBase.CollectionTypeEnum.Queryable)
				{
					array = new Expression[]
					{
						context,
						Expression.Quote(ruleLambdaExpression)
					};
				}
				else
				{
					array = new Expression[]
					{
						context,
						ruleLambdaExpression
					};
				}
				MethodInfo extensionMethod2;
				if (collectionTypeEnum == ExpressionBuilderBase.CollectionTypeEnum.Queryable)
				{
					extensionMethod2 = this.GetExtensionMethod(typeof(Queryable), pattern);
				}
				else
				{
					extensionMethod2 = this.GetExtensionMethod(typeof(Enumerable), pattern2);
					if (!context.Type.IsGenericType && !context.Type.IsArray)
					{
						MethodInfo extensionMethod3 = this.GetExtensionMethod(typeof(Enumerable), "Cast.*IEnumerable");
						context = Expression.Call(null, extensionMethod3.MakeGenericMethod(new Type[]
						{
							itemType
						}), new Expression[]
						{
							context
						});
						array[0] = context;
					}
				}
				MethodCallExpression methodCallExpression = Expression.Call(null, extensionMethod2.MakeGenericMethod(new Type[]
				{
					itemType
				}), array);
				Expression expression;
				if (@operator == "exists" || @operator == "doesNotExist")
				{
					if (this.PerformNullChecks)
					{
						expression = Expression.AndAlso(left, methodCallExpression);
					}
					else
					{
						expression = methodCallExpression;
					}
				}
				else
				{
					MethodCallExpression methodCallExpression2 = Expression.Call(null, extensionMethod.MakeGenericMethod(new Type[]
					{
						itemType
					}), new Expression[]
					{
						context
					});
					if (this.PerformNullChecks)
					{
						expression = Expression.AndAlso(Expression.AndAlso(left, methodCallExpression2), Expression.Invoke(ruleLambdaExpression, new Expression[]
						{
							methodCallExpression
						}));
					}
					else
					{
						expression = Expression.AndAlso(methodCallExpression2, Expression.Invoke(ruleLambdaExpression, new Expression[]
						{
							methodCallExpression
						}));
					}
				}
				if (@operator == "doesNotExist")
				{
					expression = Expression.Not(expression);
				}
				return expression;
			}
			throw new Exception(string.Format("Operator {0} is not surrpoted for collections", @operator));
		}

		private MethodInfo GetExtensionMethod(Type type, string pattern)
		{
			Regex rx = new Regex(pattern);
			return (from x in type.GetMethods()
			where rx.IsMatch(x.ToString())
			select x).FirstOrDefault<MethodInfo>();
		}

		private MethodInfo GetExtensionMethod(string name, Type type)
		{
			if (type == typeof(IQueryable))
			{
				return typeof(Queryable).GetMethods().Where(delegate(MethodInfo method)
				{
					if (method.Name == name)
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
				}).FirstOrDefault<MethodInfo>();
			}
			if (type == typeof(IEnumerable))
			{
				return typeof(Enumerable).GetMethods().Where(delegate(MethodInfo method)
				{
					if (method.Name == name)
					{
						return (from parameter in method.GetParameters()
						where parameter.ParameterType.Name.StartsWith("Func") && parameter.ParameterType.GetGenericArguments().Count<Type>() == 2
						select parameter).Any<ParameterInfo>();
					}
					return false;
				}).FirstOrDefault<MethodInfo>();
			}
			throw new Exception("Unsupported Type. Only IQueryable or IEnumerable are supported");
		}

		private Type GetItemType(Type type)
		{
			if (type.IsArray)
			{
				return type.GetElementType();
			}
			type.IsAssignableFrom(typeof(IEnumerable));
			if (type.IsAssignableFrom(typeof(IEnumerable<>)))
			{
				return type.GetGenericArguments()[0];
			}
			type.IsAssignableFrom(typeof(IQueryable));
			if (type.IsAssignableFrom(typeof(IQueryable<>)))
			{
				return type.GetGenericArguments()[0];
			}
			throw new Exception("Unsupported type: not an Array, IEnumerable<>, or IQueryable<>. Cannot determine item's type");
		}

		private Expression BuildNotOperator(XElement element)
		{
			return Expression.Not(this.Build(element.Elements().First<XElement>()));
		}
	}
}
