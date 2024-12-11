using System.Text;

namespace BeyondTrustConnector.Parser
{
    internal class SyslogParser
    {
        private int Position = 0;
        private List<SyslogToken> _tokens = [];
        internal SyslogParser(string log)
        {
            var lexer = new SyslogLexer(log);
            SyslogToken token;
            do
            {
                token = lexer.Lex();
                _tokens.Add(token);
            } while (token.Kind != SyslogTokenKind.EndOfLogToken);
        }

        internal SyslogToken Peek(int offset = 0)
        {
            var index = Position + offset;
            if (index >= _tokens.Count)
            {
                return new SyslogToken(SyslogTokenKind.EndOfLogToken, index, null);
            }
            return _tokens[index];
        }

        internal SyslogToken LookAhead => Peek(1);
        internal SyslogToken Current => Peek();

        internal SyslogToken NextToken()
        {
            if (Position >= _tokens.Count)
            {
                return new SyslogToken(SyslogTokenKind.EndOfLogToken, Position, "");
            }
            return _tokens[Position++];
        }
        internal SyslogToken MatchToken(SyslogTokenKind kind)
        {
            if (Position >= _tokens.Count)
            {
                return new SyslogToken(SyslogTokenKind.EndOfLogToken, Position, "");
            }
            var token = _tokens[Position++];
            if (token.Kind != kind)
            {
                return SyslogToken.BadToken(token.Position);
            }
            return token;
        }

        internal SyslogToken? SkipUntil(SyslogTokenKind kind)
        {
            SyslogToken? token = null;
            while (Current.Kind != SyslogTokenKind.EndOfLogToken)
            {
                if (Current.Kind == kind)
                {
                    token = Current;
                    break;
                }
                Position++;
            }
            Position++;
            return token;
        }

        internal Expression ParseExpression()
        {
            Expression expression = Expression.Bad;
            if (Current.Kind == SyslogTokenKind.TimestampToken)
            {
                expression = new TimestampExpression(Current.Value);
            }
            else if (Current.Kind == SyslogTokenKind.WordToken && Current.Value == "BG")
            {
                Position += 2;
                var correlationId = Current.Value;
                Position += 2;
                expression = new CorrelationExpression(int.Parse(correlationId));
            }
            else if (TryParseKeyValueExpression(out var keyValueExpression))
            {
                expression = keyValueExpression!;
            }
            else if (Current.Kind == SyslogTokenKind.WordToken)
            {
                expression = new LiteralValueExpression(Current.Value);
            }
            Position++;
            return expression;
        }

        private bool TryParseKeyValueExpression(out KeyValueExpression? expression)
        {
            if (Current.Kind != SyslogTokenKind.WordToken || LookAhead.Kind != SyslogTokenKind.EqualToken
                                                           && Peek(2).Kind != SyslogTokenKind.EqualToken)
            {
                expression = null;
                return false;
            }
            var value = new StringBuilder();

            while (Current.Kind != SyslogTokenKind.EqualToken)
            {
                value.Append(Current.Value);
                Position++;
            }
            var keyExpression = new KeyExpression(value.ToString());
            Position++;
            value.Clear();
            while (Current.Kind is not SyslogTokenKind.NewLineToken and not SyslogTokenKind.EndOfLogToken and not SyslogTokenKind.SemiColonToken)
            {
                if (Current.Kind == SyslogTokenKind.BackslashToken)
                {
                    value.Append(Current.Value);
                    value.Append(LookAhead.Value);
                    Position += 2;
                }
                value.Append(Current.Value);
                Position++;
            }
            var valueExpression = new LiteralValueExpression(value.ToString());
            expression = new KeyValueExpression(keyExpression, valueExpression);
            return true;
        }

        internal IEnumerable<Expression> Parse()
        {
            do
            {
                yield return ParseExpression();
            } while (Position < _tokens.Count);
        }
    }
}
