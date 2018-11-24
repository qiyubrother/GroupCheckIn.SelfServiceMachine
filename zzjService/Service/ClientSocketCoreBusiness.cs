using qiyubrother.MemoryMappedFileImplement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Transaction.Definition;
using NNHuman.Cipher.Standard;
using Newtonsoft.Json;

namespace zzjService
{
    public class ClientSocketCoreBusiness
    {
        public bool IsStarting { get; set; }
        public void Start()
        {
            IsStarting = true;
            byte[] recvBuffer = new byte[1024 * 50]; // 50KB
            var socketServerIP = "192.168.0.6";
            var socketServerPort = 5060;
            Socket clientSocket = null;

            LogHelper.WriteLogAsync($"[{DateTime.Now}][SocketClient][Connect to:{socketServerIP}:{socketServerPort}][初始化完成.]", LogType.All);
            do
            {
                try
                {
                    //设定服务器IP地址 
                    IPAddress ip = IPAddress.Parse(socketServerIP);
                    clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    clientSocket.Connect(new IPEndPoint(ip, socketServerPort)); //配置服务器IP与端口

                    #region 首次连接，客户端生成公钥，并将公钥发给服务端
                    var clientKey = ECDHUtil.CreatAliceKeyType();
                    var tx = new ECDHTx { ClientPublicKey = clientKey.PublicKey };
                    #region Base64加密后，追加#作为结束符
                    var j = JsonConvert.SerializeObject(tx);
                    var js = qiyubrother.extend.Base64.EncodeString(j, Encoding.UTF8) + "#";
                    #endregion
                    #region 100000 发送公钥到云端SocketServer
                    try
                    {
                        clientSocket.Send(Encoding.Default.GetBytes(js)); // 发送
                        LogHelper.WriteLogAsync($"[{DateTime.Now}][SocketClient][已经发送数据到云端SocketServer]{js}", LogType.All);
                        #region 接收响应报文，并生成客户端私钥
                        var len = clientSocket.Receive(recvBuffer); // 接收回文
                        if (len > 0)
                        {
                            var buffer = recvBuffer;
                            var rx = Encoding.UTF8.GetString(buffer, 0, len);
                            LogHelper.WriteLogAsync($"[{DateTime.Now}][SocketClient][收到SocketServer响应数据]{rx}", LogType.All);
                            #region Base64解密
                            var rxd = qiyubrother.extend.Base64.DecodeString(rx);
                            #endregion
                            var rxObj = JsonConvert.DeserializeObject<ECDHRx>(rxd);
                            // 客户端根据服务端传来的公钥，生成自己的私钥
                            ECDHUtil.CreatAlicePriKey(ref clientKey, rxObj.ServerPublicKey);
                        }
                        else
                        {
                            throw new Exception($"[异常][SocketClient]收到字节为0，疑似连接断开。");
                        }
                        #endregion
                    }
                    catch(Exception xcp)
                    {
                        throw new Exception($"[异常][SocketClient]{xcp.Message}");
                    }
                    #endregion
                    #endregion

                    Console.WriteLine($"[{DateTime.Now}]开始监听WebSocket服务...");
                    Console.WriteLine(clientKey.PublicKey);
                    var aesHelper = new AESUtil();
                    var task = new Task(() =>
                    {
                        while (true)
                        {
                            if (clientSocket.Poll(100, SelectMode.SelectRead))
                            {
                                // 服务端已经断开连接
                                Console.WriteLine($"{DateTime.Now}服务端已经断开连接...");
                                LogHelper.WriteLogAsync($"[{DateTime.Now}][SocketClient]服务端已经断开连接...", LogType.All);

                                break;
                            }
                            if (SystemEnvironment.TxQueue.Count > 0)
                            {
                                SystemEnvironment.TxQueue.TryDequeue(out QueueItem itemData);
                                var webSocketID = itemData.WebSocketID;
                                LogHelper.WriteLogAsync($"[{DateTime.Now}][SocketClient][收到WebSocketServer数据][Tx]", LogType.All);
                                var data = itemData.RawData as string;
                                #region AES加密数据
                                var aesData = aesHelper.AESEncrypt(data, clientKey.PrivateKey);
                                #endregion
                                #region Base64加密后，追加#作为结束符
                                var base64Data = qiyubrother.extend.Base64.EncodeBytes(aesData) + "#";
                                #endregion
                                #region 发送数据到云端SocketServer
                                try
                                {
                                    clientSocket.Send(Encoding.UTF8.GetBytes(base64Data)); // 发送
                                    LogHelper.WriteLogAsync($"[{DateTime.Now}][SocketClient][已经发送数据到云端SocketServer]{data}", LogType.All);
                                }
                                catch
                                {
                                    throw new Exception($"[异常][SocketClient]发送数据失败，疑似连接断开。");
                                }
                                #endregion
                                #region 接收响应报文，并写入Rx通道
                                var len = clientSocket.Receive(recvBuffer); // 接收回文
                                if (len > 0)
                                {
                                    var buffer = recvBuffer;
                                    var aesBuffer = new byte[len];
                                    Array.Copy(buffer, aesBuffer, len);

                                    #region AES解密数据
                                    var bOriginalData = aesHelper.AESDecrypt(aesBuffer, clientKey.PrivateKey);
                                    #endregion
                                    LogHelper.WriteLogAsync($"[{DateTime.Now}][SocketClient][收到SocketServer响应数据]{Encoding.UTF8.GetString(buffer, 0, len)}", LogType.All);
                                    #region 写入Rx数据到数据通道（mmfiChannelB）（未作任何加密）
                                    var strOriginalData = Encoding.UTF8.GetString(bOriginalData);
                                    SystemEnvironment.RxQueue.Enqueue(new QueueItem { RawData = strOriginalData, WebSocketID = webSocketID });
                                    LogHelper.WriteLogAsync($"[{DateTime.Now}][SocketClient][写入Rx数据到数据通道（未作任何加密）]{strOriginalData}", LogType.All);
                                    #endregion
                                }
                                else
                                {
                                    throw new Exception($"[异常][SocketClient]收到字节为0，疑似连接断开。") ;
                                }
                                #endregion
                            }
                            Thread.Sleep(100);
                        }
                    });
                    task.Start();
                    task.Wait();
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLogAsync($"[{DateTime.Now}]{ex.Message}", LogType.All);
                    Console.WriteLine($"[{DateTime.Now}]E::{ex.Message}");
                    try
                    {
                        if (clientSocket.Connected)
                        {
                            clientSocket.Shutdown(SocketShutdown.Both);
                            clientSocket.Close();
                        }
                    }
                    catch { }
                    Thread.Sleep(3000);
                    continue;
                }
            } while (IsStarting);
            LogHelper.WriteLogAsync($"[{DateTime.Now}][正常退出系统]再见。", LogType.All);
        }
    }
}
