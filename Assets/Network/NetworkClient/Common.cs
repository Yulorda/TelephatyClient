using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Telepathy
{
    public abstract class Common
    {
        public const int MESSAGE_QUEUE_SIZE_WARNING = 10000;
        public const int MAX_MESSAGE_SIZE = 16000;
        public const int SEND_TIMEOUT = 5000;
        public int ReceiveQueueCount => receiveQueue.Count;
        
        protected ConcurrentQueue<byte[]> receiveQueue = new ConcurrentQueue<byte[]>();

        public bool NoDelay = true;
        [ThreadStatic] private static byte[] header;
        [ThreadStatic] private static byte[] payload;
        
        public bool GetNextMessage(out byte[] message)
        {
            return receiveQueue.TryDequeue(out message);
        }

        protected static bool SendMessagesBlocking(NetworkStream stream, byte[][] messages, Action<NetworkLog> log = null)
        {
            try
            {
                int packetSize = 0;
                for (int i = 0; i < messages.Length; i++)
                {
                    packetSize += sizeof(int) + messages[i].Length;
                }

                if (payload == null || payload.Length < packetSize)
                    payload = new byte[packetSize];

                int position = 0;
                for (int i = 0; i < messages.Length; ++i)
                {
                    if (header == null)
                        header = new byte[4];

                    Utils.IntToBytesBigEndianNonAlloc(messages[i].Length, header);

                    Array.Copy(header, 0, payload, position, header.Length);
                    Array.Copy(messages[i], 0, payload, position + header.Length, messages[i].Length);
                    position += header.Length + messages[i].Length;
                }

                stream.Write(payload, 0, packetSize);

                return true;
            }
            catch (Exception exception)
            {
                log?.Invoke(new NetworkLog(EventType.Error, "Send: stream.Write exception: " + exception));
                return false;
            }
        }

        protected static bool ReadMessageBlocking(NetworkStream stream, int MaxMessageSize, out byte[] content, Action<NetworkLog> log = null)
        {
            content = null;

            if (header == null)
                header = new byte[4];

            if (!stream.ReadExactly(header, 4))
                return false;

            int size = Utils.BytesToIntBigEndian(header);

            if (size <= MaxMessageSize)
            {
                content = new byte[size];
                return stream.ReadExactly(content, size);
            }
            log?.Invoke(new NetworkLog(EventType.Error, "ReadMessageBlocking: possible allocation attack with a header of: " + size + " bytes."));
            return false;
        }

        protected static void ReceiveLoop(TcpClient client, ConcurrentQueue<byte[]> receiveQueue, int MaxMessageSize, Action<NetworkLog> log = null)
        {
            NetworkStream stream = client.GetStream();

            DateTime messageQueueLastWarning = DateTime.Now;

            try
            {
                log.Invoke(new NetworkLog(EventType.Connected));

                while (true)
                {
                    byte[] content;
                    if (!ReadMessageBlocking(stream, MaxMessageSize, out content, log))
                    {
                        break;
                    }

                    log?.Invoke(new NetworkLog((EventType.Data), Encoding.Default.GetString(content)));
                    receiveQueue.Enqueue(content);

                    if (receiveQueue.Count > MESSAGE_QUEUE_SIZE_WARNING)
                    {
                        TimeSpan elapsed = DateTime.Now - messageQueueLastWarning;
                        if (elapsed.TotalSeconds > 10)
                        {
                            log?.Invoke(new NetworkLog(EventType.Data, "ReceiveLoop: messageQueue is getting big(" + receiveQueue.Count + "), try calling GetNextMessage more often. You can call it more than once per frame!"));
                            messageQueueLastWarning = DateTime.Now;
                        }
                    }
                }
            }
            catch
            {
                log?.Invoke(new NetworkLog(EventType.Error, "ReceiveLoop: finished receive function for connectionId=" + " reason: "));
            }
            finally
            {
                stream.Close();
                client.Close();
            }
        }

        protected static void SendLoop(TcpClient client, SafeQueue<byte[]> sendQueue, ManualResetEvent sendPending, Action<NetworkLog> log = null)
        {
            NetworkStream stream = client.GetStream();

            try
            {
                while (client.Connected)
                {
                    sendPending.Reset();

                    byte[][] messages;
                    if (sendQueue.TryDequeueAll(out messages))
                    {
                        if (!SendMessagesBlocking(stream, messages, log))
                            break;
                    }

                    sendPending.WaitOne();
                }
            }
            catch (ThreadAbortException)
            {
                // happens on stop. don't log anything.
            }
            catch (ThreadInterruptedException)
            {
                // happens if receive thread interrupts send thread.
            }
            catch (Exception exception)
            {
                // something went wrong. the thread was interrupted or the
                // connection closed or we closed our own connection or ...
                // -> either way we should stop gracefully
                log?.Invoke(new NetworkLog(EventType.Error, "SendLoop Exception: " + " reason: " + exception));
            }
            finally
            {
                stream.Close();
                client.Close();
            }
        }
    }
}