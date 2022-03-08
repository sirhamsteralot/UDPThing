using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Net;
using UDPLibraryV2.Core;
using UDPLibraryV2.Core.Packets;
using UDPLibraryV2.RPC.Attributes;
using UDPLibraryV2.Core.PacketQueueing;
using System.Buffers;

namespace UDPLibraryV2.RPC
{
    public class RPCService
    {
        private UDPCore _core;

        private Dictionary<short, Procedure> _procedures;

        public RPCService(UDPCore core)
        {
            _core = core;

            _procedures = new Dictionary<short, Procedure>();

            _core.OnPayloadReceivedEvent += _core_OnPayloadReceivedEvent;
        }

        public async Task<ResT> CallProcedure<ReqT, ResT>(ReqT request, bool compression, IPEndPoint target, int timeoutMs)
            where ReqT : IRequest
            where ResT : IResponse
        {
            _core.QueueSerializable(request, compression, SendPriority.Medium, target);

            TaskCompletionSource<ResT> completed = new TaskCompletionSource<ResT>();
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            Action<ReconstructedPacket, IPEndPoint?> payloadReceivedCallback = (packet, ep) =>
            {
                if (packet.TypeId == request.ResponseTypeId)
                {
                    var result = (ResT)Activator.CreateInstance(typeof(ResT));
                    result.Deserialize(packet.GetPayloadBytes(), 0);

                    completed.TrySetResult(result);
                    tokenSource.Cancel();
                }
            };

            _core.OnPayloadReceivedEvent += payloadReceivedCallback;

            try
            {
                await Task.Delay(timeoutMs, tokenSource.Token);
                _core.OnPayloadReceivedEvent -= payloadReceivedCallback;
                throw new TimeoutException("Request timed out");
            }
            catch (TaskCanceledException)
            {
                // nom
            }

            ResT res = await completed.Task;

            // Cleanup
            _core.OnPayloadReceivedEvent -= payloadReceivedCallback;

            return res;
        }

        private void _core_OnPayloadReceivedEvent(ReconstructedPacket packet, IPEndPoint? source)
        {
            if (!_procedures.TryGetValue(packet.TypeId, out Procedure procedure))
                return;

            IRequest request = (IRequest)Activator.CreateInstance(procedure.requestType);
            request.Deserialize(packet.GetPayloadBytes(), 0);

            IResponse response = procedure.proc.Invoke(request);

            _core.QueueSerializable(response, response.DoCompress, SendPriority.Medium, source);
        }

        // maybe call from generated code?
        public unsafe void RegisterProcedure<ReqT, ResT>(short procedureId, Func<IRequest, IResponse> procedure)
            where ReqT : IRequest
            where ResT : IResponse
        {
            _procedures[procedureId] = new Procedure()
            {
                proc = procedure,
                requestType = typeof(ReqT),
                responseType = typeof(ResT),
            };
        }

        private void RegisterProcedures()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            IEnumerable<MethodInfo> selectedMethods = assemblies.SelectMany(x => x.GetTypes().SelectMany(y => y.GetMethods().Where(z => z.CustomAttributes.Any(a => a.AttributeType == typeof(Attributes.Procedure)))));

            foreach (var method in selectedMethods)
            {
                
            }
        }
    }
}
