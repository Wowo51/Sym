// Copyright Warren Harding 2025.
using Sym.Core;
using Sym.Atoms;
using System.Collections.Immutable;
using System.Linq;
using System;

namespace Sym.Operations
{
    public sealed class Function : Operation
    {
        public string Name { get; init; }

        public Function(string name, ImmutableList<IExpression> arguments) : base(arguments)
        {
            Name = name;
        }

        public override Shape Shape
        {
            get
            {
                Shape resultShape = Shape.Scalar;
                foreach (IExpression arg in Arguments)
                {
                    if (!arg.Shape.IsValid)
                    {
                        return Shape.Error;
                    }

                    resultShape = resultShape.CombineForElementWise(arg.Shape);
                    if (!resultShape.IsValid)
                    {
                        return Shape.Error; // Incompatible shapes found during aggregation
                    }
                }
                return resultShape;
            }
        }

        public override IExpression Canonicalize()
        {
            ImmutableList<IExpression> canonicalArgs = Arguments.Select(arg => arg.Canonicalize()).ToImmutableList();

            // Attempt to numerically evaluate known functions if arguments are numbers.
            IExpression? evaluatedResult = null;
            if (canonicalArgs.All(arg => arg is Number))
            {
                try
                {
                    // Handle single-argument functions
                    if (canonicalArgs.Count == 1)
                    {
                        double val = (double)((Number)canonicalArgs[0]).Value;
                        double result;
                        bool evaluated = true;
                        switch (Name.ToLowerInvariant())
                        {
                            case "sin": result = Math.Sin(val); break;
                            case "cos": result = Math.Cos(val); break;
                            case "tan": result = Math.Tan(val); break;
                            case "exp": result = Math.Exp(val); break;
                            case "log": result = Math.Log(val); break; // Natural log
                            default:
                                evaluated = false;
                                result = 0; // dummy
                                break;
                        }
                        if (evaluated && !double.IsNaN(result) && !double.IsInfinity(result))
                        {
                            evaluatedResult = new Number((decimal)result);
                        }
                    }
                    // Handle two-argument functions (e.g., Log(value, base))
                    else if (canonicalArgs.Count == 2 && Name.ToLowerInvariant() == "log")
                    {
                        double val = (double)((Number)canonicalArgs[0]).Value;
                        double baseVal = (double)((Number)canonicalArgs[1]).Value;
                        double result = Math.Log(val, baseVal);
                        if (!double.IsNaN(result) && !double.IsInfinity(result))
                        {
                            evaluatedResult = new Number((decimal)result);
                        }
                    }
                }
                catch (Exception)
                {
                    // On any math or conversion error, evaluatedResult remains null,
                    // and we fall through to symbolic representation.
                }
            }

            if (evaluatedResult != null)
            {
                return evaluatedResult;
            }

            // If not evaluated, reconstruct the function if its arguments have changed during canonicalization.
            bool argumentsChanged = false;
            if (Arguments.Count != canonicalArgs.Count)
            {
                argumentsChanged = true;
            }
            else
            {
                for (int i = 0; i < Arguments.Count; i++)
                {
                    if (!ReferenceEquals(Arguments[i], canonicalArgs[i]))
                    {
                        argumentsChanged = true;
                        break;
                    }
                }
            }

            if (argumentsChanged)
            {
                return new Function(Name, canonicalArgs);
            }

            return this;
        }

        public override string ToDisplayString()
        {
            return $"{Name}({string.Join(", ", Arguments.Select(arg => arg.ToDisplayString()))})";
        }

        public override bool InternalEquals(IExpression other)
        {
            if (other is not Function otherFunc || !Name.Equals(otherFunc.Name, StringComparison.Ordinal))
            {
                return false;
            }
            if (Arguments.Count != otherFunc.Arguments.Count)
            {
                return false;
            }
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (!Arguments[i].InternalEquals(otherFunc.Arguments[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public override int InternalGetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(GetType());
            hash.Add(Name);
            foreach (IExpression arg in Arguments)
            {
                hash.Add(arg);
            }
            return hash.ToHashCode();
        }

        public override Operation WithArguments(ImmutableList<IExpression> newArgs)
        {
            return new Function(Name, newArgs);
        }
    }
}