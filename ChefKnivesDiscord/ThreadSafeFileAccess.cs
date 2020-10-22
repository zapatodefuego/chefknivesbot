using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace ChefKnivesDiscord
{
    public class ThreadSafeFileAccess<T> where T : new()
    {
        private readonly string _filePath;
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public ThreadSafeFileAccess(string filePath)
        {
            _filePath = filePath;

            if (!File.Exists(_filePath))
            {
                File.Create(_filePath);
            }
        }

        public T Read()
        {
            _lock.EnterReadLock();
            try
            {
                var data = File.ReadAllText(_filePath);
                var model = JsonConvert.DeserializeObject<T>(data);

                if (model == null)
                {
                    model = new T();
                }

                return model;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Write(T model)
        {
            _lock.EnterWriteLock();
            try
            {
                var data = JsonConvert.SerializeObject(model);
                File.WriteAllText(_filePath, data);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
