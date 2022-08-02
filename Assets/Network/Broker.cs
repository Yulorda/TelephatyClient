using System;
using System.Collections.Generic;

using UniRx;

//Rename
namespace UniRxMessageBroker
{
    /// <summary>
    /// In-Memory PubSub filtered by Type.
    /// </summary>
    public class Broker : IMessageBroker, IDisposable
    {
        private interface IInvoke
        {
            void Invoke(object obj);
        }

        private class SubjectWrapper<T> : IInvoke
        {
            public ISubject<T> observable;

            public void Invoke(object obj)
            {
                observable.OnNext((T)obj);
            }
        }

        /// <summary>
        /// MessageBroker in Global scope.
        /// </summary>
        public static readonly IMessageBroker Default = new Broker();

        bool isDisposed = false;
        readonly Dictionary<Type, object> notifiers = new Dictionary<Type, object>();

        public void Publish<T>(T message)
        {
            object notifier;
            lock (notifiers)
            {
                if (isDisposed) return;

                if (!notifiers.TryGetValue(typeof(T), out notifier))
                {
                    return;
                }
            }
            (((SubjectWrapper<T>)notifier).observable).OnNext(message);
        }

        public void Publish(object value, Type type)
        {
            object notifier;
            lock (notifiers)
            {
                if (isDisposed) return;

                if (!notifiers.TryGetValue(type, out notifier))
                {
                    return;
                }
            }
           ((IInvoke)notifier).Invoke(value);
        }

        public IObservable<T> Receive<T>()
        {
            object notifier;
            lock (notifiers)
            {
                if (isDisposed) throw new ObjectDisposedException("Broker");

                if (!notifiers.TryGetValue(typeof(T), out notifier))
                {
                    SubjectWrapper<T> n = new SubjectWrapper<T>() { observable = new Subject<T>().Synchronize() };
                    notifier = n;
                    notifiers.Add(typeof(T), notifier);
                }
            }

            return ((IObservable<T>)(((SubjectWrapper<T>)notifier).observable)).AsObservable();
        }

        public void Dispose()
        {
            lock (notifiers)
            {
                if (!isDisposed)
                {
                    isDisposed = true;
                    notifiers.Clear();
                }
            }
        }
    }
}
