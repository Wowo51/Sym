// Copyright Warren Harding 2025
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SymIO.Parsing
{
    internal static class Tokenizer
    {
        private static readonly Regex _regex = new Regex(
            @"(?<Number>[0-9]+(\.[0-9]+)?m?)|" +
            @"(?<Symbol>[a-zA-Z_][a-zA-Z0-9_]*)|" +
            @"(?<Power>\*\*)|" +
            @"(?<Plus>\+)|" +
            @"(?<Minus>-)|" +
            @"(?<Star>\*)|" +
            @"(?<Slash>/)|" +
            @"(?<Equals>=)|" +
            @"(?<LeftParen>\()|" +
            @"(?<RightParen>\))|" +
            @"(?<Comma>,)|" +
            @"(?<Whitespace>\s+)",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        public static List<Token> Tokenize(string input)
        {
            List<Token> tokens = new List<Token>();
            MatchCollection matches = _regex.Matches(input);

            foreach (Match match in matches)
            {
                if (match.Groups["Whitespace"].Success) continue;

                string value = match.Value;
                if (match.Groups["Number"].Success)
                    tokens.Add(new Token(TokenType.Number, value.TrimEnd('m')));
                else if (match.Groups["Symbol"].Success)
                    tokens.Add(new Token(TokenType.Symbol, value));
                else if (match.Groups["Power"].Success)
                    tokens.Add(new Token(TokenType.Power, value));
                else if (match.Groups["Plus"].Success)
                    tokens.Add(new Token(TokenType.Plus, value));
                else if (match.Groups["Minus"].Success)
                    tokens.Add(new Token(TokenType.Minus, value));
                else if (match.Groups["Star"].Success)
                    tokens.Add(new Token(TokenType.Star, value));
                else if (match.Groups["Slash"].Success)
                    tokens.Add(new Token(TokenType.Slash, value));
                else if (match.Groups["Equals"].Success)
                    tokens.Add(new Token(TokenType.Equals, value));
                else if (match.Groups["LeftParen"].Success)
                    tokens.Add(new Token(TokenType.LeftParen, value));
                else if (match.Groups["RightParen"].Success)
                    tokens.Add(new Token(TokenType.RightParen, value));
                else if (match.Groups["Comma"].Success)
                    tokens.Add(new Token(TokenType.Comma, value));
            }

            return tokens;
        }
    }
}