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
using System.Linq.Expressions;

namespace UDPLibraryV2.RPC
{
    public class RPCService
    {
        private UDPCore _core;

        internal Dictionary<short, ProcedureRecord> procedures;

        public RPCService(UDPCore core)
        {
            _core = core;

            procedures = new Dictionary<short, ProcedureRecord>();

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
            if (!procedures.TryGetValue(packet.TypeId, out ProcedureRecord procedure))
                return;

            IRequest request = (IRequest)Activator.CreateInstance(procedure.requestType);
            request.Deserialize(packet.GetPayloadBytes(), 0);

            IResponse response = procedure.proc.Invoke(request, source);

            _core.QueueSerializable(response, response.DoCompress, SendPriority.Medium, source);
        }

        public unsafe void RegisterProcedure<ReqT, ResT>(short procedureId, Func<IRequest, IPEndPoint, IResponse> procedure)
            where ReqT : IRequest
            where ResT : IResponse
        {
            procedures[procedureId] = new ProcedureRecord()
            {
                proc = procedure,
                requestType = typeof(ReqT),
                responseType = typeof(ResT),
            };
        }

        public void FindAndRegisterProcedures()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            IEnumerable<MethodInfo> selectedMethods = assemblies.SelectMany(x => x.GetTypes().SelectMany(y => y.GetMethods(BindingFlags.Static | BindingFlags.Public).Where(z => z.CustomAttributes.Any(a => a.AttributeType == typeof(Procedure)))));

            foreach (var method in selectedMethods)
            {
                Procedure procedureAttribute = method.GetCustomAttribute<Procedure>();

                if (procedureAttribute.ProcedureId != ((IRequest)Activator.CreateInstance(procedureAttribute.RequestType)).TypeId)
                    throw new ArgumentException($"Procedure Id does not match request type id! procedureid: {procedureAttribute.ProcedureId} requestType: {nameof(procedureAttribute.RequestType)}");

                if (!procedures.TryAdd(procedureAttribute.ProcedureId, new ProcedureRecord()
                {
                    proc = (Func<IRequest, IPEndPoint, IResponse>)Delegate.CreateDelegate(typeof(Func<IRequest, IPEndPoint, IResponse>), method),
                    requestType = procedureAttribute.RequestType,
                    responseType = procedureAttribute.ResponseType,
                }))
                {
                    throw new Exception($"could not add procedure {procedureAttribute.ProcedureId}... most likely the procedureID already exists.");
                }
            }
        }
    }
}
