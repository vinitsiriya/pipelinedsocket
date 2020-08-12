using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleClient
{
    public class Program
    {

        public static async Task Main(String[] args)
        {
            EndPoint iPEnd = new IPEndPoint(IPAddress.Any, 8084);
            var serverSocket = new Socket(iPEnd.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.NoDelay = true;
            serverSocket.Bind(iPEnd);
            serverSocket.Listen(0);

            var acceptTask = serverSocket.AcceptAsync();




            EndPoint endPointClient = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8084);

            var clientSocket = new Socket(endPointClient.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true
            };
            var clientThread = clientSocket.ConnectAsync(endPointClient);
            clientThread.Wait();
            byte[] databytes = UTF8Encoding.UTF8.GetBytes("Hello hello ghellpo helloooeoeoeoeoeoeoeoe ");
         //   clientSocket.Send(databytes);
            {
             
                var memorypool = System.Buffers.SlabMemoryPoolFactory.Create();
                PipelinedSocket.PipeSocket clientPipeSocket = new PipelinedSocket.PipeSocket(clientSocket, memorypool, PipeScheduler.Inline);
                clientPipeSocket.Start();

               // byte[] databytes = UTF8Encoding.UTF8.GetBytes("Hello hello ghellpo helloooeoeoeoeoeoeoeoe ");
                ReadOnlyMemory<byte> data = new ReadOnlyMemory<byte>(databytes);
                for (int i = 0; i < 100; i++)
                {
                    await clientPipeSocket.Transport.Output.WriteAsync(data, CancellationToken.None);
                    await clientPipeSocket.Transport.Output.FlushAsync();
                }
             
                
            }
            acceptTask.Wait();


            var serverSoc = acceptTask.Result;
          
            {
                var scheduler = PipeScheduler.Inline;
                var memorypool = System.Buffers.SlabMemoryPoolFactory.Create();
                PipelinedSocket.PipeSocket pipeSocket = new PipelinedSocket.PipeSocket(serverSoc, memorypool, PipeScheduler.Inline);
                pipeSocket.Start();


                PipeReader pipeReader = pipeSocket.Transport.Input;
                while (pipeReader.TryRead(out var readResult))
                {
                    var data = readResult.Buffer;
                    var str = UTF8Encoding.UTF8.GetString(data);
                    pipeReader.AdvanceTo(data.End, data.End);
                    Console.WriteLine(str);
                }
                

          
            }

           



            Console.WriteLine("Done");
            Console.ReadKey();
            Console.ReadKey();
        }
    }
}
