using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DankWaifu.Collections
{
    public class ConcurrentHashSet<T> : ISet<T>, IReadOnlyCollection<T>, IDisposable
    {
        private bool _disposed;

        private readonly HashSet<T> _hashSet;
        private readonly SemaphoreSlim _lock;

        public ConcurrentHashSet()
        {
            _hashSet = new HashSet<T>();
            _lock = new SemaphoreSlim(1, 1);
        }

        public bool IsReadOnly => false;

        int ICollection<T>.Count => _hashSet.Count;

        int IReadOnlyCollection<T>.Count => _hashSet.Count;

        public int Count => _hashSet.Count;

        public IEnumerator<T> GetEnumerator()
        {
            return _hashSet.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            _lock.Wait();

            try
            {
                _hashSet.Add(item);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task AddAsync(T item)
        {
            await _lock.WaitAsync()
                .ConfigureAwait(false);

            try
            {
                _hashSet.Add(item);
            }
            finally
            {
                _lock.Release();
            }
        }

        public void UnionWith(IEnumerable<T> other)
        {
            _lock.Wait();

            try
            {
                _hashSet.UnionWith(other);
            }
            finally
            {
                _lock.Release();
            }
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            _lock.Wait();

            try
            {
                _hashSet.IntersectWith(other);
            }
            finally
            {
                _lock.Release();
            }
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            _lock.Wait();

            try
            {
                _hashSet.ExceptWith(other);
            }
            finally
            {
                _lock.Release();
            }
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            _lock.Wait();

            try
            {
                _hashSet.SymmetricExceptWith(other);
            }
            finally
            {
                _lock.Release();
            }
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return _hashSet.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return _hashSet.IsSupersetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return _hashSet.IsProperSupersetOf(other);
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return _hashSet.IsProperSubsetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return _hashSet.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return _hashSet.SetEquals(other);
        }

        bool ISet<T>.Add(T item)
        {
            _lock.Wait();

            try
            {
                return _hashSet.Add(item);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task ClearAsync()
        {
            await _lock.WaitAsync()
                .ConfigureAwait(false);

            try
            {
                _hashSet.Clear();
            }
            finally
            {
                _lock.Release();
            }
        }

        public void Clear()
        {
            _lock.Wait();

            try
            {
                _hashSet.Clear();
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<bool> ContainsAsync(T item)
        {
            await _lock.WaitAsync()
                .ConfigureAwait(false);

            try
            {
                return _hashSet.Contains(item);
            }
            finally
            {
                _lock.Release();
            }
        }

        public bool Contains(T item)
        {
            _lock.Wait();

            try
            {
                return _hashSet.Contains(item);
            }
            finally
            {
                _lock.Release();
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _lock.Wait();

            try
            {
                _hashSet.CopyTo(array, arrayIndex);
            }
            finally
            {
                _lock.Release();
            }
        }

        public bool Remove(T item)
        {
            _lock.Wait();

            try
            {
                return _hashSet.Remove(item);
            }
            finally
            {
                _lock.Release();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _lock.Dispose();
                _hashSet.Clear();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        ~ConcurrentHashSet()
        {
            Dispose(true);
        }
    }
}