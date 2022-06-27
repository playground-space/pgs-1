using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Oes.Snapshoting;
using Orleans;
using System.Reflection;
using System.Text.Json;

namespace Oes;

public class CreatedV1
{
    public Int64 Time { get; set; }
}

public abstract class OesGrain : Grain
{
    private Dictionary<String, MethodInfo> _reduceMethods = new();
    private Dictionary<String, Type>       _eventTypes    = new();

    private EventStoreClient?  _esdb;
    private ISnapshotStorage?  _snapshotStorage;
    private ISnapshot?         _snapshot;

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _reduceMethods = GetReduceMethods();
        _eventTypes    = GetEventTypes(_reduceMethods);

        var grainId    = this.GetGrainId();
        var snapshotId = $"{grainId.Type.ToStringUtf8()}/{grainId.Key.ToStringUtf8()}";
        var streamName = snapshotId;

        _snapshotStorage = ServiceProvider.GetRequiredService<ISnapshotStorage>();
        _esdb            = ServiceProvider.GetRequiredService<EventStoreClient>();
        _snapshot        = await _snapshotStorage.ReadAsync(snapshotId);
        
        if (_snapshot == null)
        {
            _snapshot = new Snapshot(
                version  : 0,
                revision : StreamPosition.Start,
                content  : String.Empty);
        }

        var streamReading = _esdb.ReadStreamAsync(
            direction         : Direction.Forwards,
            streamName        : streamName,
            revision          : _snapshot.Revision,
            cancellationToken : cancellationToken);

        if (await streamReading.ReadState == ReadState.StreamNotFound)
        {
        }
        else
        {
            await foreach (var @event in streamReading)
            {
                if (@event.Event.EventNumber > _snapshot!.Revision)
                {
                    var hasReduceMethod = _reduceMethods.TryGetValue(@event.Event.EventType, out var reduceMethod);
                    if (hasReduceMethod is false)
                    {
                        throw new Exception("");
                    }

                    var hasEventType = _eventTypes.TryGetValue(@event.Event.EventType, out var eventType);
                    if (hasEventType is false)
                    {
                        throw new Exception("");
                    }

                    var eventObj = JsonSerializer.Deserialize(@event.Event.Data.Span, eventType!);
                    var _        = reduceMethod!.Invoke(this, new object[] { eventObj! });
                }
            }
        }      

        await base.OnActivateAsync(cancellationToken);
    }

    protected void Dispatch<T>(T @event)
    {
        
    }

    private Dictionary<String, MethodInfo> GetReduceMethods()
    {
        var grainType    = GetType();
        var grainMethods = grainType.GetMethods();

        return grainMethods
            .Where(method =>
            {
                var parameters            = method.GetParameters();
                var hasRequiredParameters =
                    parameters.Length                == 2 &&
                    parameters[0].ParameterType.Name == "ISnapshot";

                _ = method.Name                               == "Reduce" &&
                    method.ReturnParameter.ParameterType.Name == "void";


                return true;
            })
            .ToDictionary(
                keySelector     : method => method.GetParameters()[1].ParameterType.Name,
                elementSelector : method => method);
    }

    private Dictionary<String, Type> GetEventTypes(Dictionary<String, MethodInfo> reduceMethods)
    {
        return reduceMethods.ToDictionary(
            keySelector     : method => method.Value.GetParameters()[1].ParameterType.Name,
            elementSelector : method => method.Value.GetParameters()[1].ParameterType);
    }
}