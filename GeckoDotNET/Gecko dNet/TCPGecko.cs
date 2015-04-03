using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.Win32.SafeHandles;

using D2XXDirect;
using System.IO;

namespace FTDIUSBGecko
{
    public class TCPGecko : IGeckoDevice
    {
        private IPHostEntry _ipHostInfo;
        private IPAddress   _ipAddress;
        private IPEndPoint  _remoteEP;

        private Socket _clientSocket;
        private NetworkStream _clientStream;
        private BinaryReader _recvStream;
        private BinaryWriter _sendStream;

        private int _recvTimeout = 0;
        private int _sendTimeout = 0;

        private int _readBufferSize = 8192;

        private int _latencyTimer = 16;

        public TCPGecko()
        {
            try
            {
                // Establish the remote endpoint for the socket.
                _ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                _ipAddress = _ipHostInfo.AddressList[0];

                // Create a TCP/IP  socket.
                _clientSocket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);
            }
            catch (ArgumentNullException ane)
            {
                //Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                //Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                //Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }
        }

        public FT_STATUS OpenBySerialNumber(String serial)
        {
            try
            {
                // Establish the remote endpoint for the socket.
                foreach (IPAddress ip in _ipHostInfo.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        _ipAddress = ip;
                    }
                }

                return ResetDevice();
            }
            catch (ArgumentNullException ane)
            {
                //Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                //Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                //Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }

            return FT_STATUS.FT_DEVICE_NOT_OPENED;
        }

        public FT_STATUS GetNumberOfDevices(ref UInt32 numberOfDevices)
        {
            numberOfDevices = 1;

            return FT_STATUS.FT_OK;
        }

        public FT_STATUS SetTimeouts(UInt32 readTimeout, UInt32 writeTimeout)
        {
            try
            {
                _recvTimeout = (int)readTimeout;
                _sendTimeout = (int)writeTimeout;

                // Set the socket read/write timeouts.
                _clientSocket.ReceiveTimeout = _recvTimeout;
                _clientSocket.SendTimeout = _sendTimeout;

                return FT_STATUS.FT_OK;
            }
            catch (ArgumentNullException ane)
            {
                //Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                //Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                //Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }

            return FT_STATUS.FT_DEVICE_NOT_OPENED;
        }

        public FT_STATUS SetLatencyTimer(Byte ucTimer)
        {
            _latencyTimer = (int)ucTimer;

            return FT_STATUS.FT_OK;
        }

        public FT_STATUS InTransferSize(UInt32 transfer)
        {
            try
            {
                // Set the socket receive buffer size.
                _readBufferSize = (int)transfer;

                _clientSocket.ReceiveBufferSize = _readBufferSize;

                return FT_STATUS.FT_OK;
            }
            catch (ArgumentNullException ane)
            {
                //Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                //Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                //Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }

            return FT_STATUS.FT_DEVICE_NOT_OPENED;
        }

        public FT_STATUS Close()
        {
            // Close the socket to the remote endpoint. Catch any errors.
            try
            {
                if (_clientSocket.Connected)
                {
                    // Release the socket.
                    _clientSocket.Shutdown(SocketShutdown.Both);
                    _clientSocket.Close();

                    return FT_STATUS.FT_OK;
                }
            }
            catch (ArgumentNullException ane)
            {
                //Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                //Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                //Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }

            return FT_STATUS.FT_DEVICE_NOT_OPENED;
        }

        public FT_STATUS ResetDevice()
        {
            // Connect the socket to the remote endpoint. Catch any errors.
            try
            {
                bool wasConnected = _clientSocket.Connected;

                int server_port = 0xd6ec; // "dolphin gecko"
                int connection_attempt = 0;
                while (!_clientSocket.Connected && (connection_attempt < 10))
                {
                    ++connection_attempt;

                    // Establish the remote endpoint for the socket.
                    _remoteEP = new IPEndPoint(_ipAddress, server_port);

                    try
                    {
                        // Create a TCP/IP  socket.
                        _clientSocket = new Socket(AddressFamily.InterNetwork,
                            SocketType.Stream, ProtocolType.Tcp);

                        SetTimeouts((uint)_recvTimeout, (uint)_sendTimeout);
                        InTransferSize((uint)_readBufferSize);
                        SetLatencyTimer((byte)_latencyTimer);

                        _clientSocket.Connect(_remoteEP);

                        _clientStream = new NetworkStream(_clientSocket);
                        _recvStream = new BinaryReader(_clientStream);
                        _sendStream = new BinaryWriter(_clientStream);
                    }
                    catch (ArgumentNullException ane)
                    {
                        //Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                    }
                    catch (SocketException se)
                    {
                        //Console.WriteLine("SocketException : {0}", se.ToString());
                    }
                    catch (Exception e)
                    {
                        //Console.WriteLine("Unexpected exception : {0}", e.ToString());
                    }
                    ++server_port;
                }

                if (_clientSocket.Connected)
                {
                    if (!wasConnected)
                    {
                        Console.WriteLine("Socket connected to {0}",
                            _clientSocket.RemoteEndPoint.ToString());
                    }

                    return FT_STATUS.FT_OK;
                }
            }
            catch (ArgumentNullException ane)
            {
                //Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                //Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                //Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }

            return FT_STATUS.FT_DEVICE_NOT_OPENED;
        }

        public FT_STATUS Purge(UInt32 eventCh)
        {
            const UInt32 FT_PURGE_RX = 1;
            const UInt32 FT_PURGE_TX = 2;

            //if ((eventCh & FT_PURGE_RX) != 0)
            //    _recvStream.Flush();

            if ((eventCh & FT_PURGE_TX) != 0)
                _sendStream.Flush();

            return FT_STATUS.FT_OK;
        }

        public FT_STATUS Read(Byte[] buffer, UInt32 nobytes, ref UInt32 bytes_read)
        {
            // Connect the socket to the remote endpoint. Catch any errors.
            try
            {
                if (_clientSocket.Connected)
                {
                    bytes_read = 0;

                    // Receive the response from the remote device.
                    int bytesRec = _recvStream.Read(buffer, 0, (int)nobytes); // _clientSocket.Receive(buffer, (int)nobytes, SocketFlags.None);

                    bytes_read = (UInt32)bytesRec;

                    return FT_STATUS.FT_OK;
                }
            }
            catch (ArgumentNullException ane)
            {
                //Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                //Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                //Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }

            return FT_STATUS.FT_DEVICE_NOT_OPENED;
        }

        public FT_STATUS Write(Byte[] buffer, Int32 nobytes, ref UInt32 bytes_written)
        {
            // Connect the socket to the remote endpoint. Catch any errors.
            try
            {
                if (_clientSocket.Connected)
                {
                    bytes_written = 0;

                    // Send the data through the socket.
                    //int bytesSent = _clientSocket.Send(buffer, (int)nobytes, SocketFlags.None);
                    _sendStream.Write(buffer, 0, (int)nobytes);

                    int bytesSent = nobytes;

                    bytes_written = (UInt32)bytesSent;

                    return FT_STATUS.FT_OK;
                }
            }
            catch (ArgumentNullException ane)
            {
                //Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                //Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                //Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }

            return FT_STATUS.FT_DEVICE_NOT_OPENED;
        }
    }
}
