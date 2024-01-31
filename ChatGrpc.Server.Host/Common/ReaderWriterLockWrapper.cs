namespace ChatGrpc.Server.Host.Common
{
    public class ReaderWriterLockWrapper : IDisposable
    {
        public struct WriteLockToken : IDisposable
        {
            private readonly ReaderWriterLockSlim _lock;
            public WriteLockToken(ReaderWriterLockSlim rwlock)
            {
                this._lock = rwlock;
                _lock.EnterWriteLock();
            }
            public void Dispose() => _lock.ExitWriteLock();
        }

        public struct ReadLockToken : IDisposable
        {
            private readonly ReaderWriterLockSlim _lock;
            public ReadLockToken(ReaderWriterLockSlim rwlock)
            {
                this._lock = rwlock;
                _lock.EnterReadLock();
            }
            public void Dispose() => _lock.ExitReadLock();
        }

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public ReadLockToken ReadLock() => new ReadLockToken(_lock);
        public WriteLockToken WriteLock() => new WriteLockToken(_lock);

        public void Dispose() => _lock.Dispose();
    }
}
