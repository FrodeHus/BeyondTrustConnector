using System.Diagnostics;

namespace BeyondTrustConnector.Parser;

[DebuggerDisplay("{Kind} {Value}")]
internal class SyslogToken(SyslogTokenKind kind, int position, string value)
{
    internal SyslogTokenKind Kind { get; } = kind;
    internal int Position { get; } = position;
    internal string Value { get; } = value;

    internal static SyslogToken BadToken(int position = 0) => new SyslogToken(SyslogTokenKind.BadToken, position, null);
}

internal enum SyslogTokenKind
{
    BadToken,
    TimestampToken,
    WordToken,
    NumberToken,
    WhiteSpaceToken,
    ColonToken,
    SemiColonToken,
    EqualToken,
    NewLineToken,
    CarriageReturnToken,
    EndToken,
    LeftBracketToken,
    RightBracketToken,
    CommaToken,
    QuoteToken,
    DoubleQuoteToken,
    BackslashToken,
    SlashToken,
    PeriodToken,
    DashToken,
    UnderScoreToken,
    PlusToken,
    AsteriskToken,
    LeftParenthesisToken,
    RightParenthesisToken,
    EndOfLogToken,
    AtToken
}