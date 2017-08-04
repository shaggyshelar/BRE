using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Core
{
    internal class CacheDict<TKey, TValue>
    {
        private struct KeyInfo<TKey_internal, TValue_internal>
        {
            internal readonly TValue_internal Value;

            internal readonly LinkedListNode<TKey_internal> List;

            internal KeyInfo(TValue_internal value, LinkedListNode<TKey_internal> list)
            {
                this.Value = value;
                this.List = list;
            }
        }

        private readonly Dictionary<TKey, CacheDict<TKey, TValue>.KeyInfo<TKey, TValue>> _dict;

        private readonly LinkedList<TKey> _list;

        private readonly int _maxSize;

        internal TValue this[TKey key]
        {
            get
            {
                TValue result;
                if (!this.TryGetValue(key, out result))
                {
                    throw new KeyNotFoundException();
                }
                return result;
            }
            set
            {
                this.Add(key, value);
            }
        }

        internal CacheDict(int maxSize)
        {
            this._dict = new Dictionary<TKey, CacheDict<TKey, TValue>.KeyInfo<TKey, TValue>>();
            this._list = new LinkedList<TKey>();
            this._maxSize = maxSize;
        }

        internal void Add(TKey key, TValue value)
        {
            CacheDict<TKey, TValue>.KeyInfo<TKey, TValue> keyInfo;
            if (this._dict.TryGetValue(key, out keyInfo))
            {
                this._list.Remove(keyInfo.List);
            }
            else if (this._list.Count == this._maxSize)
            {
                LinkedListNode<TKey> last = this._list.Last;
                this._list.RemoveLast();
                this._dict.Remove(last.Value);
            }
            LinkedListNode<TKey> linkedListNode = new LinkedListNode<TKey>(key);
            this._list.AddFirst(linkedListNode);
            this._dict[key] = new CacheDict<TKey, TValue>.KeyInfo<TKey, TValue>(value, linkedListNode);
        }

        internal bool TryGetValue(TKey key, out TValue value)
        {
            CacheDict<TKey, TValue>.KeyInfo<TKey, TValue> keyInfo;
            if (this._dict.TryGetValue(key, out keyInfo))
            {
                LinkedListNode<TKey> list = keyInfo.List;
                if (list.Previous != null)
                {
                    this._list.Remove(list);
                    this._list.AddFirst(list);
                }
                value = keyInfo.Value;
                return true;
            }
            value = default(TValue);
            return false;
        }
    }
}
