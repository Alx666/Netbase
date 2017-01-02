using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netbase.Shared.Collections
{
    public class ConcurrentDictionary<K, V> : ConcurrentContainer
    {
        private Dictionary<K, V> m_hDictionary;

        public ConcurrentDictionary() : base()
        {
            m_hDictionary = new Dictionary<K, V>();
        }

        public bool TryAdd(K key, V value)
        {
            bool bRes = false;

            this.SpinWaitFor(() => 
            {
                if (!m_hDictionary.ContainsKey(key))
                {
                    m_hDictionary.Add(key, value);
                    bRes = true;
                }
                else
                {
                    m_hDictionary[key] = value;
                }
            });

            return bRes;
        }

        public bool TryRemove(K key, out V value)
        {
            V removed = default(V);
            bool bRes = false;

            this.SpinWaitFor(() =>
            {
                if (m_hDictionary.ContainsKey(key))
                {
                    removed = m_hDictionary[key];
                    m_hDictionary.Remove(key);
                    bRes = true;
                }
            });

            value = removed;
            return bRes;
        }

        public V this[K key]
        {
            get
            {
                V extracted = default(V);
                bool bRes = false;

                this.SpinWaitFor(() =>
                {
                    if (m_hDictionary.ContainsKey(key))
                    {
                        extracted = m_hDictionary[key];
                        bRes = true;
                    }
                });

                if (!bRes)
                    throw new KeyNotFoundException();

                return extracted;
            }

            set
            {
                this.TryAdd(key, value);
            }
        }
    }
}
