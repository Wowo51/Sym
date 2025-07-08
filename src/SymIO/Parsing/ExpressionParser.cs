// Copyright Warren Harding 2025
using Sym.Core;
using Sym.Atoms;
using Sym.Operations;
using System.Collections.Immutable;
using System.Globalization;
using System.Collections.Generic;
using System;

namespace SymIO.Parsing
{
    /// <summary>
    /// A simple recursive descent parser for the C# pseudo-language.
    /// </summary>
    internal class ExpressionParser
    {
        private List<Token> _tokens = new List<Token>();
        private int _position;

        public IExpression Parse(string input)
        {
            _tokens = Tokenizer.Tokenize(input);
            _position = 0;
            IExpression expr = ParseEquality();
            if (_position != _tokens.Count)
                throw new ArgumentException("Invalid expression: unexpected tokens at the end.");
            return expr;
        }

        private IExpression ParseEquality()
        {
            IExpression left = ParseExpression();
            if (Match(TokenType.Equals))
            {
                IExpression right = ParseExpression();
                return new Equality(left, right);
            }
            return left;
        }

        private IExpression ParseExpression()
        {
            IExpression expr = ParseTerm();
            while (Match(TokenType.Plus, TokenType.Minus))
            {
                Token op = Previous();
                IExpression right = ParseTerm();
                if (op.Type == TokenType.Plus)
                    expr = new Add(expr, right);
                else
                    expr = new Subtract(expr, right);
            }
            return expr;
        }

        private IExpression ParseTerm()
        {
            IExpression expr = ParseUnary(); // PATCH: Call ParseUnary to handle unary minus correctly
            while (Match(TokenType.Star, TokenType.Slash))
            {
                Token op = Previous();
                IExpression right = ParseUnary(); // PATCH: Call ParseUnary here as well
                if (op.Type == TokenType.Star)
                    expr = new Multiply(expr, right);
                else
                    expr = new Divide(expr, right);
            }
            return expr;
        }

        // PATCH: New method to handle unary operators.
        // This gives unary minus higher precedence than multiplication/division but lower than power.
        private IExpression ParseUnary()
        {
            if (Match(TokenType.Minus))
            {
                IExpression right = ParseUnary();
                return new Multiply(new Number(-1m), right);
            }
            // A unary plus is ignored.
            if (Match(TokenType.Plus))
            {
                return ParseUnary();
            }
            return ParsePower();
        }

        // PATCH: Renamed from ParseFactor to ParsePower for clarity.
        private IExpression ParsePower()
        {
            IExpression expr = ParseAtom();
            if (Match(TokenType.Power))
            {
                // Call ParseUnary for the right-hand side to correctly handle right-associativity
                // and expressions like `a ** -b`.
                IExpression right = ParseUnary();
                expr = new Power(expr, right);
            }
            return expr;
        }

        private IExpression ParseAtom()
        {
            if (Match(TokenType.Number))
            {
                return new Number(decimal.Parse(Previous().Value, CultureInfo.InvariantCulture));
            }
            if (Match(TokenType.Symbol))
            {
                string name = Previous().Value;
                if (Match(TokenType.LeftParen)) // It's a function call
                {
                    List<IExpression> args = new List<IExpression>();
                    if (!Check(TokenType.RightParen))
                    {
                        do
                        {
                            // BUGFIX: Changed from ParseEquality to ParseExpression
                            // to prevent equations from being passed as function arguments.
                            args.Add(ParseExpression());
                        } while (Match(TokenType.Comma));
                    }
                    Consume(TokenType.RightParen, "Expected ')' after arguments.");

                    // Handle special constructors
                    switch (name.ToLowerInvariant())
                    {
                        case "vector":
                            return new Vector(args.ToImmutableList());
                        case "matrix":
                            List<Vector> rowVectors = new List<Vector>();
                            foreach (IExpression arg in args)
                            {
                                if (arg is Vector vector)
                                {
                                    rowVectors.Add(vector);
                                }
                                else
                                {
                                    throw new ArgumentException("All arguments for a Matrix must be Vectors.");
                                }
                            }
                            return new Matrix(rowVectors.ToImmutableList());
                        case "derivative":
                            if (args.Count != 2) throw new ArgumentException("Derivative function expects 2 arguments.");
                            return new Derivative(args[0], args[1]);
                        case "integral":
                            if (args.Count != 2) throw new ArgumentException("Integral function expects 2 arguments.");
                            return new Integral(args[0], args[1]);
                        case "grad":
                            if (args.Count != 2) throw new ArgumentException("Grad function expects 2 arguments.");
                            return new Grad(args[0], args[1]);
                        case "div":
                            if (args.Count != 2) throw new ArgumentException("Div function expects 2 arguments.");
                            return new Div(args[0], args[1]);
                        case "curl":
                            if (args.Count != 2) throw new ArgumentException("Curl function expects 2 arguments.");
                            return new Curl(args[0], args[1]);
                        default:
                            return new Function(name, args.ToImmutableList());
                    }
                }
                return new Symbol(name);
            }

            // PATCH: Removed the unary minus handling from here as it's now in ParseUnary.

            if (Match(TokenType.LeftParen))
            {
                IExpression expr = ParseEquality();
                Consume(TokenType.RightParen, "Expected ')' after expression.");
                return expr;
            }
            throw new ArgumentException($"Unexpected token: {Peek().Type}");
        }

        private bool Match(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        private bool Check(TokenType type)
        {
            if (_position >= _tokens.Count) return false;
            return Peek().Type == type;
        }

        private Token Advance()
        {
            if (_position < _tokens.Count) _position++;
            return Previous();
        }

        private Token Peek()
        {
            if (_position >= _tokens.Count)
            {
                return new Token(TokenType.EOF, string.Empty);
            }
            return _tokens[_position];
        }
        private Token Previous() => _tokens[_position - 1];

        private Token Consume(TokenType type, string message)
        {
            if (Check(type)) return Advance();
            throw new ArgumentException(message);
        }
    }
}