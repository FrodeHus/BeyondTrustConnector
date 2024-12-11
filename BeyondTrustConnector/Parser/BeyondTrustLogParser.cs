namespace BeyondTrustConnector.Parser;

internal class BeyondTrustLogParser(string log)
{
    public List<BeyondTrustLogEntry> Parse()
    {
        var entries = new List<BeyondTrustLogEntry>();
        var parser = new SyslogParser(log);
        var expressions = parser.Parse().Where(entry => entry.Kind != ExpressionKind.BadExpression);
        BeyondTrustLogEntry? entry = null;
        foreach (var expression in expressions)
        {
            if (expression.Kind == ExpressionKind.TimestampExpression)
            {
                if (entry != null)
                {
                    entries.Add(entry);
                }
                entry = new BeyondTrustLogEntry
                {
                    Timestamp = ((TimestampExpression)expression).Value!.Value
                };
            }
            if (entry == null)
            {
                continue;
            }
            if (expression.Kind == ExpressionKind.KeyValueExpression)
            {
                var keyValue = (KeyValueExpression)expression;
                entry.Details.Add(((KeyExpression)keyValue.Key).Key, ((LiteralValueExpression)keyValue.Value).Value);
            }
            if(expression.Kind == ExpressionKind.CorrelationExpression)
            {
                entry.CorrelationId = ((CorrelationExpression)expression).CorrelationId;
            }
        }
        if (entry != null)
        {
            entries.Add(entry);
        }
        return entries;
    }
}