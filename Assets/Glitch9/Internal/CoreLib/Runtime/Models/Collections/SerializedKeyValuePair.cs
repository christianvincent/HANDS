using System;

namespace Glitch9.Collections
{
    [Serializable]
    public class SerializedKeyValuePair<TKey, TValue>
    {
        public TKey key;
        public TValue value;
    }
}