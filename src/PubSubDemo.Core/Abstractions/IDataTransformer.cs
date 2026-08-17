namespace PubSubDemo.Core.Abstractions;

/// <summary>
/// The "transform" step of the pipeline: turns raw input data into whatever
/// shape subscribers actually want to consume. Deliberately synchronous and
/// side-effect-free from the caller's point of view, which is what makes it
/// trivial to unit test in isolation from the bus and the subscribers.
/// </summary>
public interface IDataTransformer<in TIn, out TOut>
{
    TOut Transform(TIn input);
}
