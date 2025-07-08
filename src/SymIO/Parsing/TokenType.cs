// Copyright Warren Harding 2025
namespace SymIO.Parsing
{
    internal enum TokenType
    {
        Number, Symbol,
        Plus, Minus, Star, Slash, Power, Equals,
        LeftParen, RightParen, Comma,
        EOF
    }
}