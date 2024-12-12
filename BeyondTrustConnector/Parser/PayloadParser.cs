namespace BeyondTrustConnector.Parser
{
    internal class PayloadParser(string payload)
    {
        private int _position = 0;
        private char Peek(int offset = 0)
        {
            var index = _position + offset;
            if (index >= payload.Length)
            {
                return '\0';
            }
            return payload[index];
        }
        private char LookAhead => Peek(1);
        private char Current => Peek();

        internal IEnumerable<(string Key, string Value)> Parse()
        {
            do
            {
                var key = ParseKey();
                var value = ParseValue();
                yield return (key, value);
            } while (Current != '\0' && Current != ';');
        }

        private string ParseValue()
        {
            var start = _position;
            do
            {
                if (Current == '\\' && LookAhead == ';')
                {
                    _position += 2;
                }
                _position++;
            } while (Current != ';' && Current != '\0');

            var value = payload[start.._position];
            if(Current == ';')
            {
                _position++;
            }
            return value;
        }

        private string ParseKey()
        {
            var start = _position;
            do
            {
                _position++;
            } while (Current != '=');
            var key = payload[start.._position];
            _position++;
            return key;
        }
    }
}
