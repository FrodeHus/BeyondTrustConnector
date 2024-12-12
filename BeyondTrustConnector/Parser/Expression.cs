using System.Diagnostics;

namespace BeyondTrustConnector.Parser;

internal class PayloadExpression: Expression
{
    internal List<SyslogToken> Tokens { get; } = [];
    public override ExpressionKind Kind => ExpressionKind.PayloadExpression;
}

[DebuggerDisplay("{SiteId}:{SegmentId}:{SegmentCount}")]
internal class SegmentInformationExpression(int siteId, int segmentId, int segmentCount) : Expression
{
    internal int SiteId { get; } = siteId;
    internal int SegmentId { get; } = segmentId;
    internal int SegmentCount { get; } = segmentCount;
    public override ExpressionKind Kind => ExpressionKind.SegmentInformationExpression;
}
[DebuggerDisplay("{CorrelationId}")]
internal class CorrelationExpression(int correlationId) : Expression
{
    internal int CorrelationId { get; } = correlationId;
    public override ExpressionKind Kind => ExpressionKind.CorrelationExpression;
}

[DebuggerDisplay("{Key} = {Value}")]
internal class KeyValueExpression(Expression key, Expression value) : Expression
{
    internal Expression Key { get; } = key;
    internal Expression Value { get; } = value;
    public override ExpressionKind Kind => ExpressionKind.KeyValueExpression;
}

[DebuggerDisplay("{Key}")]
internal class KeyExpression(string key) : Expression
{
    internal string Key { get; } = key;
    public override ExpressionKind Kind => ExpressionKind.KeyExpression;
}

[DebuggerDisplay("{Value}")]
internal class LiteralValueExpression(string value) : Expression
{
    internal string Value { get; } = value;
    public override ExpressionKind Kind => ExpressionKind.LiteralValueExpression;
}

[DebuggerDisplay("{Value}")]
internal class TimestampExpression(string value) : Expression
{
    private static readonly string[] _months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    internal DateTime? Value { get; } = TryParseTimestamp(value);
    public override ExpressionKind Kind => ExpressionKind.TimestampExpression;
    private static DateTime? TryParseTimestamp(string value)
    {
        try
        {
            var month = value[..3];
            var day = int.Parse(value.Substring(4, 2));
            var hour = int.Parse(value.Substring(7, 2));
            var minute = int.Parse(value.Substring(10, 2));
            var second = int.Parse(value.Substring(13, 2));
            var year = DateTime.Now.Year;
            return new DateTime(year, Array.IndexOf(_months, month) + 1, day, hour, minute, second);
        }
        catch
        {
            return null;
        }
    }
}
internal class BadExpression : Expression
{
    public override ExpressionKind Kind => ExpressionKind.BadExpression;
}

internal abstract class Expression
{
    public static Expression Bad => new BadExpression();

    public abstract ExpressionKind Kind { get; }
}


internal enum ExpressionKind
{
    BadExpression,
    TimestampExpression,
    LiteralValueExpression,
    KeyValueExpression,
    KeyExpression,
    CorrelationExpression,
    SegmentInformationExpression,
    PayloadExpression
}