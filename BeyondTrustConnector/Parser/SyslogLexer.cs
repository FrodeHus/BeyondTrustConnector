using System.Diagnostics;

namespace BeyondTrustConnector.Parser
{
    [DebuggerDisplay("P:{Position}, ch: {Current}")]
    internal class SyslogLexer(string log)
    {
        private string[] _months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
        private int Position = 0;
        internal char Peek(int offset = 0)
        {
            var index = Position + offset;
            if (index >= log.Length)
            {
                return '\0';
            }
            return log[index];
        }
        internal char LookAhead => Peek(1);
        internal char Current => Peek();
        internal void Next() => Position++;
        internal SyslogToken Lex()
        {
            if (Position >= log.Length)
            {
                return new SyslogToken(SyslogTokenKind.EndOfLogToken, Position, null);
            }

            if (TryParseTimestamp(out var token))
            {
                return token;
            }

            if (char.IsLetter(Current))
            {
                var allowedDelimiters = new char[] { '-', '_', ':' };
                var start = Position;
                do
                {
                    Next();
                } while (char.IsLetter(Current) || allowedDelimiters.Contains(Current));
                return new SyslogToken(SyslogTokenKind.WordToken, start, log[start..Position]);
            }

            if (char.IsDigit(Current))
            {
                var start = Position;
                do
                {
                    Next();
                } while (char.IsDigit(Current));
                return new SyslogToken(SyslogTokenKind.NumberToken, start, log[start..Position]);
            }

            if (Current == '\r')
            {
                return new SyslogToken(SyslogTokenKind.CarriageReturnToken, Position++, "\r");
            }

            if (Current == '\n')
            {
                return new SyslogToken(SyslogTokenKind.NewLineToken, Position++, "\n");
            }

            if (char.IsWhiteSpace(Current))
            {
                var start = Position;
                do
                {
                    Next();
                } while (char.IsWhiteSpace(Current));
                return new SyslogToken(SyslogTokenKind.WhiteSpaceToken, start, log[start..Position]);
            }

            return Current switch
            {
                ':' => new SyslogToken(SyslogTokenKind.ColonToken, Position++, ":"),
                ';' => new SyslogToken(SyslogTokenKind.SemiColonToken, Position++, ";"),
                '=' => new SyslogToken(SyslogTokenKind.EqualToken, Position++, "="),
                '[' => new SyslogToken(SyslogTokenKind.LeftBracketToken, Position++, "["),
                ']' => new SyslogToken(SyslogTokenKind.RightBracketToken, Position++, "]"),
                ',' => new SyslogToken(SyslogTokenKind.CommaToken, Position++, ","),
                '"' => new SyslogToken(SyslogTokenKind.QuoteToken, Position++, "\""),
                '\\' => new SyslogToken(SyslogTokenKind.BackslashToken, Position++, "\\"),
                '/' => new SyslogToken(SyslogTokenKind.SlashToken, Position++, "/"),
                '.' => new SyslogToken(SyslogTokenKind.PeriodToken, Position++, "."),
                '-' => new SyslogToken(SyslogTokenKind.DashToken, Position++, "-"),
                '_' => new SyslogToken(SyslogTokenKind.UnderScoreToken, Position++, "_"),
                '+' => new SyslogToken(SyslogTokenKind.PlusToken, Position++, "+"),
                '*' => new SyslogToken(SyslogTokenKind.AsteriskToken, Position++, "*"),
                '(' => new SyslogToken(SyslogTokenKind.LeftParenthesisToken, Position++, "("),
                ')' => new SyslogToken(SyslogTokenKind.RightParenthesisToken, Position++, ")"),
                '@' => new SyslogToken(SyslogTokenKind.AtToken, Position++, "@"),
                _ => new SyslogToken(SyslogTokenKind.BadToken, Position++, "")
            };
        }



        internal bool TryParseTimestamp(out SyslogToken timestampToken)
        {
            var start = Position;
            if (start + 3 >= log.Length)
            {
                timestampToken = new SyslogToken(SyslogTokenKind.BadToken, Position, null);
                return false;
            }

            if (!_months.Contains(log[Position..(Position + 3)]))
            {
                timestampToken = new SyslogToken(SyslogTokenKind.BadToken, Position, null);
                return false;
            }
            Position += 3;
            while (!char.IsLetter(Current))
            {
                Position++;
            }
            var value = log[start..Position];
            value = value.TrimEnd();
            Position--; // rewind to not include ending whitespace
            timestampToken = new SyslogToken(SyslogTokenKind.TimestampToken, start, value);
            return true;
        }
    }
}
