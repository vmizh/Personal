﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Tracing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ServiceStack.Text;
//Internalize to avoid conflicts

public static class MemoryStreamFactory
{
    public static RecyclableMemoryStreamManager RecyclableInstance = new RecyclableMemoryStreamManager();
    public static bool UseRecyclableMemoryStream { get; set; }

    public static MemoryStream GetStream()
    {
        return UseRecyclableMemoryStream
            ? RecyclableInstance.GetStream()
            : new MemoryStream();
    }

    public static MemoryStream GetStream(int capacity)
    {
        return UseRecyclableMemoryStream
            ? RecyclableInstance.GetStream(nameof(MemoryStreamFactory), capacity)
            : new MemoryStream(capacity);
    }

    public static MemoryStream GetStream(byte[] bytes)
    {
        return UseRecyclableMemoryStream
            ? RecyclableInstance.GetStream(nameof(MemoryStreamFactory), bytes, 0, bytes.Length)
            : new MemoryStream(bytes, 0, bytes.Length, true, true);
    }

    public static MemoryStream GetStream(byte[] bytes, int index, int count)
    {
        return UseRecyclableMemoryStream
            ? RecyclableInstance.GetStream(nameof(MemoryStreamFactory), bytes, index, count)
            : new MemoryStream(bytes, index, count, true, true);
    }
}

#if !NETCORE && !NET8_0
public enum EventLevel
{
    LogAlways = 0,
    Critical,
    Error,
    Warning,
    Informational,
    Verbose
}

public enum EventKeywords : long
{
    None = 0x0
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class EventSourceAttribute : Attribute
{
    public string Name { get; set; }
    public string Guid { get; set; }
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class EventAttribute : Attribute
{
    public EventAttribute(int id)
    {
    }

    public EventLevel Level { get; set; }
}

public class EventSource
{
    public void WriteEvent(params object[] unused)
    {
    }

    public bool IsEnabled()
    {
        return false;
    }

    public bool IsEnabled(EventLevel level, EventKeywords keywords)
    {
        return false;
    }
}
#endif

public sealed partial class RecyclableMemoryStreamManager
{
    /// <summary>
    ///     ETW events for RecyclableMemoryStream
    /// </summary>
    [EventSource(Name = "Microsoft-IO-RecyclableMemoryStream", Guid = "{B80CD4E4-890E-468D-9CBA-90EB7C82DFC7}")]
    public sealed class Events : EventSource
    {
        /// <summary>
        ///     Type of buffer
        /// </summary>
        public enum MemoryStreamBufferType
        {
            /// <summary>
            ///     Small block buffer
            /// </summary>
            Small,

            /// <summary>
            ///     Large pool buffer
            /// </summary>
            Large
        }

        /// <summary>
        ///     The possible reasons for discarding a buffer
        /// </summary>
        public enum MemoryStreamDiscardReason
        {
            /// <summary>
            ///     Buffer was too large to be re-pooled
            /// </summary>
            TooLarge,

            /// <summary>
            ///     There are enough free bytes in the pool
            /// </summary>
            EnoughFree
        }

        /// <summary>
        ///     Static log object, through which all events are written.
        /// </summary>
        public static Events Writer = new Events();

        /// <summary>
        ///     Logged when a stream object is created.
        /// </summary>
        /// <param name="guid">A unique ID for this stream.</param>
        /// <param name="tag">A temporary ID for this stream, usually indicates current usage.</param>
        /// <param name="requestedSize">Requested size of the stream</param>
        [Event(1, Level = EventLevel.Verbose)]
        public void MemoryStreamCreated(Guid guid, string tag, int requestedSize)
        {
            if (IsEnabled(EventLevel.Verbose, EventKeywords.None))
                WriteEvent(1, guid, tag ?? string.Empty, requestedSize);
        }

        /// <summary>
        ///     Logged when the stream is disposed
        /// </summary>
        /// <param name="guid">A unique ID for this stream.</param>
        /// <param name="tag">A temporary ID for this stream, usually indicates current usage.</param>
        [Event(2, Level = EventLevel.Verbose)]
        public void MemoryStreamDisposed(Guid guid, string tag)
        {
            if (IsEnabled(EventLevel.Verbose, EventKeywords.None)) WriteEvent(2, guid, tag ?? string.Empty);
        }

        /// <summary>
        ///     Logged when the stream is disposed for the second time.
        /// </summary>
        /// <param name="guid">A unique ID for this stream.</param>
        /// <param name="tag">A temporary ID for this stream, usually indicates current usage.</param>
        /// <param name="allocationStack">Call stack of initial allocation.</param>
        /// <param name="disposeStack1">Call stack of the first dispose.</param>
        /// <param name="disposeStack2">Call stack of the second dispose.</param>
        /// <remarks>Note: Stacks will only be populated if RecyclableMemoryStreamManager.GenerateCallStacks is true.</remarks>
        [Event(3, Level = EventLevel.Critical)]
        public void MemoryStreamDoubleDispose(Guid guid, string tag, string allocationStack, string disposeStack1,
            string disposeStack2)
        {
            if (IsEnabled())
                WriteEvent(3, guid, tag ?? string.Empty, allocationStack ?? string.Empty,
                    disposeStack1 ?? string.Empty, disposeStack2 ?? string.Empty);
        }

        /// <summary>
        ///     Logged when a stream is finalized.
        /// </summary>
        /// <param name="guid">A unique ID for this stream.</param>
        /// <param name="tag">A temporary ID for this stream, usually indicates current usage.</param>
        /// <param name="allocationStack">Call stack of initial allocation.</param>
        /// <remarks>Note: Stacks will only be populated if RecyclableMemoryStreamManager.GenerateCallStacks is true.</remarks>
        [Event(4, Level = EventLevel.Error)]
        public void MemoryStreamFinalized(Guid guid, string tag, string allocationStack)
        {
            if (IsEnabled()) WriteEvent(4, guid, tag ?? string.Empty, allocationStack ?? string.Empty);
        }

        /// <summary>
        ///     Logged when ToArray is called on a stream.
        /// </summary>
        /// <param name="guid">A unique ID for this stream.</param>
        /// <param name="tag">A temporary ID for this stream, usually indicates current usage.</param>
        /// <param name="stack">Call stack of the ToArray call.</param>
        /// <param name="size">Length of stream</param>
        /// <remarks>Note: Stacks will only be populated if RecyclableMemoryStreamManager.GenerateCallStacks is true.</remarks>
        [Event(5, Level = EventLevel.Verbose)]
        public void MemoryStreamToArray(Guid guid, string tag, string stack, int size)
        {
            if (IsEnabled(EventLevel.Verbose, EventKeywords.None))
                WriteEvent(5, guid, tag ?? string.Empty, stack ?? string.Empty, size);
        }

        /// <summary>
        ///     Logged when the RecyclableMemoryStreamManager is initialized.
        /// </summary>
        /// <param name="blockSize">Size of blocks, in bytes.</param>
        /// <param name="largeBufferMultiple">Size of the large buffer multiple, in bytes.</param>
        /// <param name="maximumBufferSize">Maximum buffer size, in bytes.</param>
        [Event(6, Level = EventLevel.Informational)]
        public void MemoryStreamManagerInitialized(int blockSize, int largeBufferMultiple, int maximumBufferSize)
        {
            if (IsEnabled()) WriteEvent(6, blockSize, largeBufferMultiple, maximumBufferSize);
        }

        /// <summary>
        ///     Logged when a new block is created.
        /// </summary>
        /// <param name="smallPoolInUseBytes">Number of bytes in the small pool currently in use.</param>
        [Event(7, Level = EventLevel.Verbose)]
        public void MemoryStreamNewBlockCreated(long smallPoolInUseBytes)
        {
            if (IsEnabled(EventLevel.Verbose, EventKeywords.None)) WriteEvent(7, smallPoolInUseBytes);
        }

        /// <summary>
        ///     Logged when a new large buffer is created.
        /// </summary>
        /// <param name="requiredSize">Requested size</param>
        /// <param name="largePoolInUseBytes">Number of bytes in the large pool in use.</param>
        [Event(8, Level = EventLevel.Verbose)]
        public void MemoryStreamNewLargeBufferCreated(int requiredSize, long largePoolInUseBytes)
        {
            if (IsEnabled(EventLevel.Verbose, EventKeywords.None)) WriteEvent(8, requiredSize, largePoolInUseBytes);
        }

        /// <summary>
        ///     Logged when a buffer is created that is too large to pool.
        /// </summary>
        /// <param name="requiredSize">Size requested by the caller</param>
        /// <param name="tag">A temporary ID for this stream, usually indicates current usage.</param>
        /// <param name="allocationStack">Call stack of the requested stream.</param>
        /// <remarks>Note: Stacks will only be populated if RecyclableMemoryStreamManager.GenerateCallStacks is true.</remarks>
        [Event(9, Level = EventLevel.Verbose)]
        public void MemoryStreamNonPooledLargeBufferCreated(int requiredSize, string tag, string allocationStack)
        {
            if (IsEnabled(EventLevel.Verbose, EventKeywords.None))
                WriteEvent(9, requiredSize, tag ?? string.Empty, allocationStack ?? string.Empty);
        }

        /// <summary>
        ///     Logged when a buffer is discarded (not put back in the pool, but given to GC to clean up).
        /// </summary>
        /// <param name="bufferType">Type of the buffer being discarded.</param>
        /// <param name="tag">A temporary ID for this stream, usually indicates current usage.</param>
        /// <param name="reason">Reason for the discard.</param>
        [Event(10, Level = EventLevel.Warning)]
        public void MemoryStreamDiscardBuffer(MemoryStreamBufferType bufferType, string tag,
            MemoryStreamDiscardReason reason)
        {
            if (IsEnabled()) WriteEvent(10, bufferType, tag ?? string.Empty, reason);
        }

        /// <summary>
        ///     Logged when a stream grows beyond the maximum capacity.
        /// </summary>
        /// <param name="requestedCapacity">The requested capacity.</param>
        /// <param name="maxCapacity">Maximum capacity, as configured by RecyclableMemoryStreamManager.</param>
        /// <param name="tag">A temporary ID for this stream, usually indicates current usage.</param>
        /// <param name="allocationStack">Call stack for the capacity request.</param>
        /// <remarks>Note: Stacks will only be populated if RecyclableMemoryStreamManager.GenerateCallStacks is true.</remarks>
        [Event(11, Level = EventLevel.Error)]
        public void MemoryStreamOverCapacity(int requestedCapacity, long maxCapacity, string tag,
            string allocationStack)
        {
            if (IsEnabled())
                WriteEvent(11, requestedCapacity, maxCapacity, tag ?? string.Empty, allocationStack ?? string.Empty);
        }
    }
}

/// <summary>
///     Manages pools of RecyclableMemoryStream objects.
/// </summary>
/// <remarks>
///     There are two pools managed in here. The small pool contains same-sized buffers that are handed to streams
///     as they write more data.
///     For scenarios that need to call GetBuffer(), the large pool contains buffers of various sizes, all
///     multiples/exponentials of LargeBufferMultiple (1 MB by default). They are split by size to avoid overly-wasteful
///     buffer
///     usage. There should be far fewer 8 MB buffers than 1 MB buffers, for example.
/// </remarks>
public partial class RecyclableMemoryStreamManager
{
    /// <summary>
    ///     Generic delegate for handling events without any arguments.
    /// </summary>
    public delegate void EventHandler();

    /// <summary>
    ///     Delegate for handling large buffer discard reports.
    /// </summary>
    /// <param name="reason">Reason the buffer was discarded.</param>
    public delegate void LargeBufferDiscardedEventHandler(Events.MemoryStreamDiscardReason reason);

    /// <summary>
    ///     Delegate for handling reports of stream size when streams are allocated
    /// </summary>
    /// <param name="bytes">Bytes allocated.</param>
    public delegate void StreamLengthReportHandler(long bytes);

    /// <summary>
    ///     Delegate for handling periodic reporting of memory use statistics.
    /// </summary>
    /// <param name="smallPoolInUseBytes">Bytes currently in use in the small pool.</param>
    /// <param name="smallPoolFreeBytes">Bytes currently free in the small pool.</param>
    /// <param name="largePoolInUseBytes">Bytes currently in use in the large pool.</param>
    /// <param name="largePoolFreeBytes">Bytes currently free in the large pool.</param>
    public delegate void UsageReportEventHandler(
        long smallPoolInUseBytes, long smallPoolFreeBytes, long largePoolInUseBytes, long largePoolFreeBytes);

    /// <summary>
    ///     Default block size, in bytes
    /// </summary>
    public const int DefaultBlockSize = 128 * 1024;

    /// <summary>
    ///     Default large buffer multiple, in bytes
    /// </summary>
    public const int DefaultLargeBufferMultiple = 1024 * 1024;

    /// <summary>
    ///     Default maximum buffer size, in bytes
    /// </summary>
    public const int DefaultMaximumBufferSize = 128 * 1024 * 1024;

    private readonly long[] largeBufferFreeSize;
    private readonly long[] largeBufferInUseSize;


    private readonly ConcurrentStack<byte[]>[] largePools;

    private readonly ConcurrentStack<byte[]> smallPool;

    private long smallPoolFreeSize;
    private long smallPoolInUseSize;

    /// <summary>
    ///     Initializes the memory manager with the default block/buffer specifications.
    /// </summary>
    public RecyclableMemoryStreamManager()
        : this(DefaultBlockSize, DefaultLargeBufferMultiple, DefaultMaximumBufferSize, false)
    {
    }

    /// <summary>
    ///     Initializes the memory manager with the given block requiredSize.
    /// </summary>
    /// <param name="blockSize">Size of each block that is pooled. Must be > 0.</param>
    /// <param name="largeBufferMultiple">Each large buffer will be a multiple of this value.</param>
    /// <param name="maximumBufferSize">Buffers larger than this are not pooled</param>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     blockSize is not a positive number, or largeBufferMultiple is not a
    ///     positive number, or maximumBufferSize is less than blockSize.
    /// </exception>
    /// <exception cref="ArgumentException">maximumBufferSize is not a multiple of largeBufferMultiple</exception>
    public RecyclableMemoryStreamManager(int blockSize, int largeBufferMultiple, int maximumBufferSize)
        : this(blockSize, largeBufferMultiple, maximumBufferSize, false)
    {
    }

    /// <summary>
    ///     Initializes the memory manager with the given block requiredSize.
    /// </summary>
    /// <param name="blockSize">Size of each block that is pooled. Must be > 0.</param>
    /// <param name="largeBufferMultiple">Each large buffer will be a multiple/exponential of this value.</param>
    /// <param name="maximumBufferSize">Buffers larger than this are not pooled</param>
    /// <param name="useExponentialLargeBuffer">Switch to exponential large buffer allocation strategy</param>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     blockSize is not a positive number, or largeBufferMultiple is not a
    ///     positive number, or maximumBufferSize is less than blockSize.
    /// </exception>
    /// <exception cref="ArgumentException">maximumBufferSize is not a multiple/exponential of largeBufferMultiple</exception>
    public RecyclableMemoryStreamManager(int blockSize, int largeBufferMultiple, int maximumBufferSize,
        bool useExponentialLargeBuffer)
    {
        if (blockSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(blockSize), blockSize, "blockSize must be a positive number");

        if (largeBufferMultiple <= 0)
            throw new ArgumentOutOfRangeException(nameof(largeBufferMultiple),
                "largeBufferMultiple must be a positive number");

        if (maximumBufferSize < blockSize)
            throw new ArgumentOutOfRangeException(nameof(maximumBufferSize),
                "maximumBufferSize must be at least blockSize");

        BlockSize = blockSize;
        LargeBufferMultiple = largeBufferMultiple;
        MaximumBufferSize = maximumBufferSize;
        UseExponentialLargeBuffer = useExponentialLargeBuffer;

        if (!IsLargeBufferSize(maximumBufferSize))
            throw new ArgumentException(string.Format("maximumBufferSize is not {0} of largeBufferMultiple",
                    UseExponentialLargeBuffer ? "an exponential" : "a multiple"),
                nameof(maximumBufferSize));

        smallPool = new ConcurrentStack<byte[]>();
        var numLargePools = useExponentialLargeBuffer
            // ReSharper disable once PossibleLossOfFraction
            ? (int)Math.Log(maximumBufferSize / largeBufferMultiple, 2) + 1
            : maximumBufferSize / largeBufferMultiple;

        // +1 to store size of bytes in use that are too large to be pooled
        largeBufferInUseSize = new long[numLargePools + 1];
        largeBufferFreeSize = new long[numLargePools];

        largePools = new ConcurrentStack<byte[]>[numLargePools];

        for (var i = 0; i < largePools.Length; ++i) largePools[i] = new ConcurrentStack<byte[]>();

        Events.Writer.MemoryStreamManagerInitialized(blockSize, largeBufferMultiple, maximumBufferSize);
    }

    /// <summary>
    ///     The size of each block. It must be set at creation and cannot be changed.
    /// </summary>
    public int BlockSize { get; }

    /// <summary>
    ///     All buffers are multiples/exponentials of this number. It must be set at creation and cannot be changed.
    /// </summary>
    public int LargeBufferMultiple { get; }

    /// <summary>
    ///     Use multiple large buffer allocation strategy. It must be set at creation and cannot be changed.
    /// </summary>
    public bool UseMultipleLargeBuffer => !UseExponentialLargeBuffer;

    /// <summary>
    ///     Use exponential large buffer allocation strategy. It must be set at creation and cannot be changed.
    /// </summary>
    public bool UseExponentialLargeBuffer { get; }

    /// <summary>
    ///     Gets the maximum buffer size.
    /// </summary>
    /// <remarks>
    ///     Any buffer that is returned to the pool that is larger than this will be
    ///     discarded and garbage collected.
    /// </remarks>
    public int MaximumBufferSize { get; }

    /// <summary>
    ///     Number of bytes in small pool not currently in use
    /// </summary>
    public long SmallPoolFreeSize => smallPoolFreeSize;

    /// <summary>
    ///     Number of bytes currently in use by stream from the small pool
    /// </summary>
    public long SmallPoolInUseSize => smallPoolInUseSize;

    /// <summary>
    ///     Number of bytes in large pool not currently in use
    /// </summary>
    public long LargePoolFreeSize
    {
        get
        {
            long sum = 0;
            foreach (var freeSize in largeBufferFreeSize) sum += freeSize;

            return sum;
        }
    }

    /// <summary>
    ///     Number of bytes currently in use by streams from the large pool
    /// </summary>
    public long LargePoolInUseSize
    {
        get
        {
            long sum = 0;
            foreach (var inUseSize in largeBufferInUseSize) sum += inUseSize;

            return sum;
        }
    }

    /// <summary>
    ///     How many blocks are in the small pool
    /// </summary>
    public long SmallBlocksFree => smallPool.Count;

    /// <summary>
    ///     How many buffers are in the large pool
    /// </summary>
    public long LargeBuffersFree
    {
        get
        {
            long free = 0;
            foreach (var pool in largePools) free += pool.Count;
            return free;
        }
    }

    /// <summary>
    ///     How many bytes of small free blocks to allow before we start dropping
    ///     those returned to us.
    /// </summary>
    public long MaximumFreeSmallPoolBytes { get; set; }

    /// <summary>
    ///     How many bytes of large free buffers to allow before we start dropping
    ///     those returned to us.
    /// </summary>
    public long MaximumFreeLargePoolBytes { get; set; }

    /// <summary>
    ///     Maximum stream capacity in bytes. Attempts to set a larger capacity will
    ///     result in an exception.
    /// </summary>
    /// <remarks>A value of 0 indicates no limit.</remarks>
    public long MaximumStreamCapacity { get; set; }

    /// <summary>
    ///     Whether to save callstacks for stream allocations. This can help in debugging.
    ///     It should NEVER be turned on generally in production.
    /// </summary>
    public bool GenerateCallStacks { get; set; }

    /// <summary>
    ///     Whether dirty buffers can be immediately returned to the buffer pool. E.g. when GetBuffer() is called on
    ///     a stream and creates a single large buffer, if this setting is enabled, the other blocks will be returned
    ///     to the buffer pool immediately.
    ///     Note when enabling this setting that the user is responsible for ensuring that any buffer previously
    ///     retrieved from a stream which is subsequently modified is not used after modification (as it may no longer
    ///     be valid).
    /// </summary>
    public bool AggressiveBufferReturn { get; set; }

    /// <summary>
    ///     Causes an exception to be thrown if ToArray is ever called.
    /// </summary>
    /// <remarks>
    ///     Calling ToArray defeats the purpose of a pooled buffer. Use this property to discover code that is calling ToArray.
    ///     If this is
    ///     set and stream.ToArray() is called, a NotSupportedException will be thrown.
    /// </remarks>
    public bool ThrowExceptionOnToArray { get; set; }

    /// <summary>
    ///     Removes and returns a single block from the pool.
    /// </summary>
    /// <returns>A byte[] array</returns>
    internal byte[] GetBlock()
    {
        byte[] block;
        if (!smallPool.TryPop(out block))
        {
            // We'll add this back to the pool when the stream is disposed
            // (unless our free pool is too large)
            block = new byte[BlockSize];
            Events.Writer.MemoryStreamNewBlockCreated(smallPoolInUseSize);
            ReportBlockCreated();
        }
        else
        {
            Interlocked.Add(ref smallPoolFreeSize, -BlockSize);
        }

        Interlocked.Add(ref smallPoolInUseSize, BlockSize);
        return block;
    }

    /// <summary>
    ///     Returns a buffer of arbitrary size from the large buffer pool. This buffer
    ///     will be at least the requiredSize and always be a multiple/exponential of largeBufferMultiple.
    /// </summary>
    /// <param name="requiredSize">The minimum length of the buffer</param>
    /// <param name="tag">The tag of the stream returning this buffer, for logging if necessary.</param>
    /// <returns>A buffer of at least the required size.</returns>
    internal byte[] GetLargeBuffer(int requiredSize, string tag)
    {
        requiredSize = RoundToLargeBufferSize(requiredSize);

        var poolIndex = GetPoolIndex(requiredSize);

        byte[] buffer;
        if (poolIndex < largePools.Length)
        {
            if (!largePools[poolIndex].TryPop(out buffer))
            {
                buffer = new byte[requiredSize];

                Events.Writer.MemoryStreamNewLargeBufferCreated(requiredSize, LargePoolInUseSize);
                ReportLargeBufferCreated();
            }
            else
            {
                Interlocked.Add(ref largeBufferFreeSize[poolIndex], -buffer.Length);
            }
        }
        else
        {
            // Buffer is too large to pool. They get a new buffer.

            // We still want to track the size, though, and we've reserved a slot
            // in the end of the inuse array for nonpooled bytes in use.
            poolIndex = largeBufferInUseSize.Length - 1;

            // We still want to round up to reduce heap fragmentation.
            buffer = new byte[requiredSize];
            string callStack = null;
            if (GenerateCallStacks)
                // Grab the stack -- we want to know who requires such large buffers
                callStack = Environment.StackTrace;
            Events.Writer.MemoryStreamNonPooledLargeBufferCreated(requiredSize, tag, callStack);
            ReportLargeBufferCreated();
        }

        Interlocked.Add(ref largeBufferInUseSize[poolIndex], buffer.Length);

        return buffer;
    }

    private int RoundToLargeBufferSize(int requiredSize)
    {
        if (UseExponentialLargeBuffer)
        {
            var pow = 1;
            while (LargeBufferMultiple * pow < requiredSize) pow <<= 1;
            return LargeBufferMultiple * pow;
        }

        return (requiredSize + LargeBufferMultiple - 1) / LargeBufferMultiple * LargeBufferMultiple;
    }

    private bool IsLargeBufferSize(int value)
    {
        return value != 0 && (UseExponentialLargeBuffer
            ? value == RoundToLargeBufferSize(value)
            : value % LargeBufferMultiple == 0);
    }

    private int GetPoolIndex(int length)
    {
        if (UseExponentialLargeBuffer)
        {
            var index = 0;
            while (LargeBufferMultiple << index < length) ++index;
            return index;
        }

        return length / LargeBufferMultiple - 1;
    }

    /// <summary>
    ///     Returns the buffer to the large pool
    /// </summary>
    /// <param name="buffer">The buffer to return.</param>
    /// <param name="tag">The tag of the stream returning this buffer, for logging if necessary.</param>
    /// <exception cref="ArgumentNullException">buffer is null</exception>
    /// <exception cref="ArgumentException">
    ///     buffer.Length is not a multiple/exponential of LargeBufferMultiple (it did not
    ///     originate from this pool)
    /// </exception>
    internal void ReturnLargeBuffer(byte[] buffer, string tag)
    {
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));

        if (!IsLargeBufferSize(buffer.Length))
            throw new ArgumentException(
                string.Format("buffer did not originate from this memory manager. The size is not {0} of ",
                    UseExponentialLargeBuffer ? "an exponential" : "a multiple") +
                LargeBufferMultiple);

        var poolIndex = GetPoolIndex(buffer.Length);

        if (poolIndex < largePools.Length)
        {
            if ((largePools[poolIndex].Count + 1) * buffer.Length <= MaximumFreeLargePoolBytes ||
                MaximumFreeLargePoolBytes == 0)
            {
                largePools[poolIndex].Push(buffer);
                Interlocked.Add(ref largeBufferFreeSize[poolIndex], buffer.Length);
            }
            else
            {
                Events.Writer.MemoryStreamDiscardBuffer(Events.MemoryStreamBufferType.Large, tag,
                    Events.MemoryStreamDiscardReason.EnoughFree);
                ReportLargeBufferDiscarded(Events.MemoryStreamDiscardReason.EnoughFree);
            }
        }
        else
        {
            // This is a non-poolable buffer, but we still want to track its size for inuse
            // analysis. We have space in the inuse array for this.
            poolIndex = largeBufferInUseSize.Length - 1;

            Events.Writer.MemoryStreamDiscardBuffer(Events.MemoryStreamBufferType.Large, tag,
                Events.MemoryStreamDiscardReason.TooLarge);
            ReportLargeBufferDiscarded(Events.MemoryStreamDiscardReason.TooLarge);
        }

        Interlocked.Add(ref largeBufferInUseSize[poolIndex], -buffer.Length);

        ReportUsageReport(smallPoolInUseSize, smallPoolFreeSize, LargePoolInUseSize,
            LargePoolFreeSize);
    }

    /// <summary>
    ///     Returns the blocks to the pool
    /// </summary>
    /// <param name="blocks">Collection of blocks to return to the pool</param>
    /// <param name="tag">The tag of the stream returning these blocks, for logging if necessary.</param>
    /// <exception cref="ArgumentNullException">blocks is null</exception>
    /// <exception cref="ArgumentException">blocks contains buffers that are the wrong size (or null) for this memory manager</exception>
    internal void ReturnBlocks(ICollection<byte[]> blocks, string tag)
    {
        if (blocks == null) throw new ArgumentNullException(nameof(blocks));

        var bytesToReturn = blocks.Count * BlockSize;
        Interlocked.Add(ref smallPoolInUseSize, -bytesToReturn);

        foreach (var block in blocks)
            if (block == null || block.Length != BlockSize)
                throw new ArgumentException("blocks contains buffers that are not BlockSize in length");

        foreach (var block in blocks)
            if (MaximumFreeSmallPoolBytes == 0 || SmallPoolFreeSize < MaximumFreeSmallPoolBytes)
            {
                Interlocked.Add(ref smallPoolFreeSize, BlockSize);
                smallPool.Push(block);
            }
            else
            {
                Events.Writer.MemoryStreamDiscardBuffer(Events.MemoryStreamBufferType.Small, tag,
                    Events.MemoryStreamDiscardReason.EnoughFree);
                ReportBlockDiscarded();
                break;
            }

        ReportUsageReport(smallPoolInUseSize, smallPoolFreeSize, LargePoolInUseSize,
            LargePoolFreeSize);
    }

    internal void ReportBlockCreated()
    {
        BlockCreated?.Invoke();
    }

    internal void ReportBlockDiscarded()
    {
        BlockDiscarded?.Invoke();
    }

    internal void ReportLargeBufferCreated()
    {
        LargeBufferCreated?.Invoke();
    }

    internal void ReportLargeBufferDiscarded(Events.MemoryStreamDiscardReason reason)
    {
        LargeBufferDiscarded?.Invoke(reason);
    }

    internal void ReportStreamCreated()
    {
        StreamCreated?.Invoke();
    }

    internal void ReportStreamDisposed()
    {
        StreamDisposed?.Invoke();
    }

    internal void ReportStreamFinalized()
    {
        StreamFinalized?.Invoke();
    }

    internal void ReportStreamLength(long bytes)
    {
        StreamLength?.Invoke(bytes);
    }

    internal void ReportStreamToArray()
    {
        StreamConvertedToArray?.Invoke();
    }

    internal void ReportUsageReport(
        long smallPoolInUseBytes, long smallPoolFreeBytes, long largePoolInUseBytes, long largePoolFreeBytes)
    {
        UsageReport?.Invoke(smallPoolInUseBytes, smallPoolFreeBytes, largePoolInUseBytes, largePoolFreeBytes);
    }

    /// <summary>
    ///     Retrieve a new MemoryStream object with no tag and a default initial capacity.
    /// </summary>
    /// <returns>A MemoryStream.</returns>
    public MemoryStream GetStream()
    {
        return new RecyclableMemoryStream(this);
    }

    /// <summary>
    ///     Retrieve a new MemoryStream object with no tag and a default initial capacity.
    /// </summary>
    /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
    /// <returns>A MemoryStream.</returns>
    public MemoryStream GetStream(Guid id)
    {
        return new RecyclableMemoryStream(this, id);
    }

    /// <summary>
    ///     Retrieve a new MemoryStream object with the given tag and a default initial capacity.
    /// </summary>
    /// <param name="tag">A tag which can be used to track the source of the stream.</param>
    /// <returns>A MemoryStream.</returns>
    public MemoryStream GetStream(string tag)
    {
        return new RecyclableMemoryStream(this, tag);
    }

    /// <summary>
    ///     Retrieve a new MemoryStream object with the given tag and a default initial capacity.
    /// </summary>
    /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
    /// <param name="tag">A tag which can be used to track the source of the stream.</param>
    /// <returns>A MemoryStream.</returns>
    public MemoryStream GetStream(Guid id, string tag)
    {
        return new RecyclableMemoryStream(this, id, tag);
    }

    /// <summary>
    ///     Retrieve a new MemoryStream object with the given tag and at least the given capacity.
    /// </summary>
    /// <param name="tag">A tag which can be used to track the source of the stream.</param>
    /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
    /// <returns>A MemoryStream.</returns>
    public MemoryStream GetStream(string tag, int requiredSize)
    {
        return new RecyclableMemoryStream(this, tag, requiredSize);
    }

    /// <summary>
    ///     Retrieve a new MemoryStream object with the given tag and at least the given capacity.
    /// </summary>
    /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
    /// <param name="tag">A tag which can be used to track the source of the stream.</param>
    /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
    /// <returns>A MemoryStream.</returns>
    public MemoryStream GetStream(Guid id, string tag, int requiredSize)
    {
        return new RecyclableMemoryStream(this, id, tag, requiredSize);
    }

    /// <summary>
    ///     Retrieve a new MemoryStream object with the given tag and at least the given capacity, possibly using
    ///     a single contiguous underlying buffer.
    /// </summary>
    /// <remarks>
    ///     Retrieving a MemoryStream which provides a single contiguous buffer can be useful in situations
    ///     where the initial size is known and it is desirable to avoid copying data between the smaller underlying
    ///     buffers to a single large one. This is most helpful when you know that you will always call GetBuffer
    ///     on the underlying stream.
    /// </remarks>
    /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
    /// <param name="tag">A tag which can be used to track the source of the stream.</param>
    /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
    /// <param name="asContiguousBuffer">Whether to attempt to use a single contiguous buffer.</param>
    /// <returns>A MemoryStream.</returns>
    public MemoryStream GetStream(Guid id, string tag, int requiredSize, bool asContiguousBuffer)
    {
        if (!asContiguousBuffer || requiredSize <= BlockSize) return GetStream(id, tag, requiredSize);

        return new RecyclableMemoryStream(this, id, tag, requiredSize, GetLargeBuffer(requiredSize, tag));
    }

    /// <summary>
    ///     Retrieve a new MemoryStream object with the given tag and at least the given capacity, possibly using
    ///     a single contiguous underlying buffer.
    /// </summary>
    /// <remarks>
    ///     Retrieving a MemoryStream which provides a single contiguous buffer can be useful in situations
    ///     where the initial size is known and it is desirable to avoid copying data between the smaller underlying
    ///     buffers to a single large one. This is most helpful when you know that you will always call GetBuffer
    ///     on the underlying stream.
    /// </remarks>
    /// <param name="tag">A tag which can be used to track the source of the stream.</param>
    /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
    /// <param name="asContiguousBuffer">Whether to attempt to use a single contiguous buffer.</param>
    /// <returns>A MemoryStream.</returns>
    public MemoryStream GetStream(string tag, int requiredSize, bool asContiguousBuffer)
    {
        return GetStream(Guid.NewGuid(), tag, requiredSize, asContiguousBuffer);
    }

    /// <summary>
    ///     Retrieve a new MemoryStream object with the given tag and with contents copied from the provided
    ///     buffer. The provided buffer is not wrapped or used after construction.
    /// </summary>
    /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
    /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
    /// <param name="tag">A tag which can be used to track the source of the stream.</param>
    /// <param name="buffer">The byte buffer to copy data from.</param>
    /// <param name="offset">The offset from the start of the buffer to copy from.</param>
    /// <param name="count">The number of bytes to copy from the buffer.</param>
    /// <returns>A MemoryStream.</returns>
    public MemoryStream GetStream(Guid id, string tag, byte[] buffer, int offset, int count)
    {
        RecyclableMemoryStream stream = null;
        try
        {
            stream = new RecyclableMemoryStream(this, id, tag, count);
            stream.Write(buffer, offset, count);
            stream.Position = 0;
            return stream;
        }
        catch
        {
            stream?.Dispose();
            throw;
        }
    }

    /// <summary>
    ///     Retrieve a new MemoryStream object with the contents copied from the provided
    ///     buffer. The provided buffer is not wrapped or used after construction.
    /// </summary>
    /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
    /// <param name="buffer">The byte buffer to copy data from.</param>
    /// <returns>A MemoryStream.</returns>
    public MemoryStream GetStream(byte[] buffer)
    {
        return GetStream(null, buffer, 0, buffer.Length);
    }


    /// <summary>
    ///     Retrieve a new MemoryStream object with the given tag and with contents copied from the provided
    ///     buffer. The provided buffer is not wrapped or used after construction.
    /// </summary>
    /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
    /// <param name="tag">A tag which can be used to track the source of the stream.</param>
    /// <param name="buffer">The byte buffer to copy data from.</param>
    /// <param name="offset">The offset from the start of the buffer to copy from.</param>
    /// <param name="count">The number of bytes to copy from the buffer.</param>
    /// <returns>A MemoryStream.</returns>
    public MemoryStream GetStream(string tag, byte[] buffer, int offset, int count)
    {
        return GetStream(Guid.NewGuid(), tag, buffer, offset, count);
    }

#if (NETCORE  || NET8_0) && !NETSTANDARD2_0
    /// <summary>
    /// Retrieve a new MemoryStream object with the given tag and with contents copied from the provided
    /// buffer. The provided buffer is not wrapped or used after construction.
    /// </summary>
    /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
    /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
    /// <param name="tag">A tag which can be used to track the source of the stream.</param>
    /// <param name="buffer">The byte buffer to copy data from.</param>
    /// <returns>A MemoryStream.</returns>
    public MemoryStream GetStream(Guid id, string tag, Memory<byte> buffer)
    {
        RecyclableMemoryStream stream = null;
        try
        {
            stream = new RecyclableMemoryStream(this, id, tag, buffer.Length);
            stream.Write(buffer.Span);
            stream.Position = 0;
            return stream;
        }
        catch
        {
            stream?.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Retrieve a new MemoryStream object with the contents copied from the provided
    /// buffer. The provided buffer is not wrapped or used after construction.
    /// </summary>
    /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
    /// <param name="buffer">The byte buffer to copy data from.</param>
    /// <returns>A MemoryStream.</returns>
    public MemoryStream GetStream(Memory<byte> buffer)
    {
        return GetStream(null, buffer);
    }

    /// <summary>
    /// Retrieve a new MemoryStream object with the given tag and with contents copied from the provided
    /// buffer. The provided buffer is not wrapped or used after construction.
    /// </summary>
    /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
    /// <param name="tag">A tag which can be used to track the source of the stream.</param>
    /// <param name="buffer">The byte buffer to copy data from.</param>
    /// <returns>A MemoryStream.</returns>
    public MemoryStream GetStream(string tag, Memory<byte> buffer)
    {
        return GetStream(Guid.NewGuid(), tag, buffer);
    }
#endif
    /// <summary>
    ///     Triggered when a new block is created.
    /// </summary>
    public event EventHandler BlockCreated;

    /// <summary>
    ///     Triggered when a new block is created.
    /// </summary>
    public event EventHandler BlockDiscarded;

    /// <summary>
    ///     Triggered when a new large buffer is created.
    /// </summary>
    public event EventHandler LargeBufferCreated;

    /// <summary>
    ///     Triggered when a new stream is created.
    /// </summary>
    public event EventHandler StreamCreated;

    /// <summary>
    ///     Triggered when a stream is disposed.
    /// </summary>
    public event EventHandler StreamDisposed;

    /// <summary>
    ///     Triggered when a stream is finalized.
    /// </summary>
    public event EventHandler StreamFinalized;

    /// <summary>
    ///     Triggered when a stream is finalized.
    /// </summary>
    public event StreamLengthReportHandler StreamLength;

    /// <summary>
    ///     Triggered when a user converts a stream to array.
    /// </summary>
    public event EventHandler StreamConvertedToArray;

    /// <summary>
    ///     Triggered when a large buffer is discarded, along with the reason for the discard.
    /// </summary>
    public event LargeBufferDiscardedEventHandler LargeBufferDiscarded;

    /// <summary>
    ///     Periodically triggered to report usage statistics.
    /// </summary>
    public event UsageReportEventHandler UsageReport;
}

/// <summary>
///     MemoryStream implementation that deals with pooling and managing memory streams which use potentially large
///     buffers.
/// </summary>
/// <remarks>
///     This class works in tandem with the RecyclableMemoryStreamManager to supply MemoryStream
///     objects to callers, while avoiding these specific problems:
///     1. LOH allocations - since all large buffers are pooled, they will never incur a Gen2 GC
///     2. Memory waste - A standard memory stream doubles its size when it runs out of room. This
///     leads to continual memory growth as each stream approaches the maximum allowed size.
///     3. Memory copying - Each time a MemoryStream grows, all the bytes are copied into new buffers.
///     This implementation only copies the bytes when GetBuffer is called.
///     4. Memory fragmentation - By using homogeneous buffer sizes, it ensures that blocks of memory
///     can be easily reused.
///     The stream is implemented on top of a series of uniformly-sized blocks. As the stream's length grows,
///     additional blocks are retrieved from the memory manager. It is these blocks that are pooled, not the stream
///     object itself.
///     The biggest wrinkle in this implementation is when GetBuffer() is called. This requires a single
///     contiguous buffer. If only a single block is in use, then that block is returned. If multiple blocks
///     are in use, we retrieve a larger buffer from the memory manager. These large buffers are also pooled,
///     split by size--they are multiples/exponentials of a chunk size (1 MB by default).
///     Once a large buffer is assigned to the stream the small blocks are NEVER again used for this stream. All operations
///     take place on the
///     large buffer. The large buffer can be replaced by a larger buffer from the pool as needed. All blocks and large
///     buffers
///     are maintained in the stream until the stream is disposed (unless AggressiveBufferReturn is enabled in the stream
///     manager).
/// </remarks>
public sealed class RecyclableMemoryStream : MemoryStream
{
    private const long MaxStreamLength = int.MaxValue;

    private static readonly byte[] emptyArray = new byte[0];

    /// <summary>
    ///     All of these blocks must be the same size
    /// </summary>
    private readonly List<byte[]> blocks = new List<byte[]>(1);

    private readonly Guid id;

    private readonly RecyclableMemoryStreamManager memoryManager;

    private readonly string tag;

    /// <summary>
    ///     This list is used to store buffers once they're replaced by something larger.
    ///     This is for the cases where you have users of this class that may hold onto the buffers longer
    ///     than they should and you want to prevent race conditions which could corrupt the data.
    /// </summary>
    private List<byte[]> dirtyBuffers;

    // long to allow Interlocked.Read (for .NET Standard 1.4 compat)
    private long disposedState;

    /// <summary>
    ///     This is only set by GetBuffer() if the necessary buffer is larger than a single block size, or on
    ///     construction if the caller immediately requests a single large buffer.
    /// </summary>
    /// <remarks>
    ///     If this field is non-null, it contains the concatenation of the bytes found in the individual
    ///     blocks. Once it is created, this (or a larger) largeBuffer will be used for the life of the stream.
    /// </remarks>
    private byte[] largeBuffer;

    /// <summary>
    ///     Unique identifier for this stream across its entire lifetime
    /// </summary>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    internal Guid Id
    {
        get
        {
            CheckDisposed();
            return id;
        }
    }

    /// <summary>
    ///     A temporary identifier for the current usage of this stream.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    internal string Tag
    {
        get
        {
            CheckDisposed();
            return tag;
        }
    }

    /// <summary>
    ///     Gets the memory manager being used by this stream.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    internal RecyclableMemoryStreamManager MemoryManager
    {
        get
        {
            CheckDisposed();
            return memoryManager;
        }
    }

    /// <summary>
    ///     Callstack of the constructor. It is only set if MemoryManager.GenerateCallStacks is true,
    ///     which should only be in debugging situations.
    /// </summary>
    internal string AllocationStack { get; }

    /// <summary>
    ///     Callstack of the Dispose call. It is only set if MemoryManager.GenerateCallStacks is true,
    ///     which should only be in debugging situations.
    /// </summary>
    internal string DisposeStack { get; private set; }

    #region Constructors

    /// <summary>
    ///     Allocate a new RecyclableMemoryStream object.
    /// </summary>
    /// <param name="memoryManager">The memory manager</param>
    public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager)
        : this(memoryManager, Guid.NewGuid(), null, 0, null)
    {
    }

    /// <summary>
    ///     Allocate a new RecyclableMemoryStream object.
    /// </summary>
    /// <param name="memoryManager">The memory manager</param>
    /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
    public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, Guid id)
        : this(memoryManager, id, null, 0, null)
    {
    }

    /// <summary>
    ///     Allocate a new RecyclableMemoryStream object
    /// </summary>
    /// <param name="memoryManager">The memory manager</param>
    /// <param name="tag">A string identifying this stream for logging and debugging purposes</param>
    public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, string tag)
        : this(memoryManager, Guid.NewGuid(), tag, 0, null)
    {
    }

    /// <summary>
    ///     Allocate a new RecyclableMemoryStream object
    /// </summary>
    /// <param name="memoryManager">The memory manager</param>
    /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
    /// <param name="tag">A string identifying this stream for logging and debugging purposes</param>
    public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, Guid id, string tag)
        : this(memoryManager, id, tag, 0, null)
    {
    }

    /// <summary>
    ///     Allocate a new RecyclableMemoryStream object
    /// </summary>
    /// <param name="memoryManager">The memory manager</param>
    /// <param name="tag">A string identifying this stream for logging and debugging purposes</param>
    /// <param name="requestedSize">The initial requested size to prevent future allocations</param>
    public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, string tag, int requestedSize)
        : this(memoryManager, Guid.NewGuid(), tag, requestedSize, null)
    {
    }

    /// <summary>
    ///     Allocate a new RecyclableMemoryStream object
    /// </summary>
    /// <param name="memoryManager">The memory manager</param>
    /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
    /// <param name="tag">A string identifying this stream for logging and debugging purposes</param>
    /// <param name="requestedSize">The initial requested size to prevent future allocations</param>
    public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, Guid id, string tag, int requestedSize)
        : this(memoryManager, id, tag, requestedSize, null)
    {
    }

    /// <summary>
    ///     Allocate a new RecyclableMemoryStream object
    /// </summary>
    /// <param name="memoryManager">The memory manager</param>
    /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
    /// <param name="tag">A string identifying this stream for logging and debugging purposes</param>
    /// <param name="requestedSize">The initial requested size to prevent future allocations</param>
    /// <param name="initialLargeBuffer">
    ///     An initial buffer to use. This buffer will be owned by the stream and returned to the
    ///     memory manager upon Dispose.
    /// </param>
    internal RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, Guid id, string tag, int requestedSize,
        byte[] initialLargeBuffer)
        : base(emptyArray)
    {
        this.memoryManager = memoryManager;
        this.id = id;
        this.tag = tag;

        if (requestedSize < memoryManager.BlockSize) requestedSize = memoryManager.BlockSize;

        if (initialLargeBuffer == null)
            EnsureCapacity(requestedSize);
        else
            largeBuffer = initialLargeBuffer;

        if (this.memoryManager.GenerateCallStacks) AllocationStack = Environment.StackTrace;

        RecyclableMemoryStreamManager.Events.Writer.MemoryStreamCreated(this.id, this.tag, requestedSize);
        this.memoryManager.ReportStreamCreated();
    }

    #endregion

    #region Dispose and Finalize

    /// <summary>
    ///     The finalizer will be called when a stream is not disposed properly.
    /// </summary>
    /// <remarks>
    ///     Failing to dispose indicates a bug in the code using streams. Care should be taken to properly account for
    ///     stream lifetime.
    /// </remarks>
    ~RecyclableMemoryStream()
    {
        Dispose(false);
    }

    /// <summary>
    ///     Returns the memory used by this stream back to the pool.
    /// </summary>
    /// <param name="disposing">Whether we're disposing (true), or being called by the finalizer (false)</param>
    [SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly",
        Justification = "We have different disposal semantics, so SuppressFinalize is in a different spot.")]
    protected override void Dispose(bool disposing)
    {
        if (Interlocked.CompareExchange(ref disposedState, 1, 0) != 0)
        {
            string doubleDisposeStack = null;
            if (memoryManager.GenerateCallStacks) doubleDisposeStack = Environment.StackTrace;

            RecyclableMemoryStreamManager.Events.Writer.MemoryStreamDoubleDispose(id, tag,
                AllocationStack, DisposeStack, doubleDisposeStack);
            return;
        }

        RecyclableMemoryStreamManager.Events.Writer.MemoryStreamDisposed(id, tag);

        if (memoryManager.GenerateCallStacks) DisposeStack = Environment.StackTrace;

        if (disposing)
        {
            memoryManager.ReportStreamDisposed();

            GC.SuppressFinalize(this);
        }
        else
        {
            // We're being finalized.

            RecyclableMemoryStreamManager.Events.Writer.MemoryStreamFinalized(id, tag, AllocationStack);

            if (AppDomain.CurrentDomain.IsFinalizingForUnload())
            {
                // If we're being finalized because of a shutdown, don't go any further.
                // We have no idea what's already been cleaned up. Triggering events may cause
                // a crash.
                base.Dispose(false);
                return;
            }

            memoryManager.ReportStreamFinalized();
        }

        memoryManager.ReportStreamLength(length);

        if (largeBuffer != null) memoryManager.ReturnLargeBuffer(largeBuffer, tag);

        if (dirtyBuffers != null)
            foreach (var buffer in dirtyBuffers)
                memoryManager.ReturnLargeBuffer(buffer, tag);

        memoryManager.ReturnBlocks(blocks, tag);
        blocks.Clear();

        base.Dispose(disposing);
    }

    /// <summary>
    ///     Equivalent to Dispose
    /// </summary>
    public override void Close()
    {
        Dispose(true);
    }

    #endregion

    #region MemoryStream overrides

    /// <summary>
    ///     Gets or sets the capacity
    /// </summary>
    /// <remarks>
    ///     Capacity is always in multiples of the memory manager's block size, unless
    ///     the large buffer is in use.  Capacity never decreases during a stream's lifetime.
    ///     Explicitly setting the capacity to a lower value than the current value will have no effect.
    ///     This is because the buffers are all pooled by chunks and there's little reason to
    ///     allow stream truncation.
    ///     Writing past the current capacity will cause Capacity to automatically increase, until MaximumStreamCapacity is
    ///     reached.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    public override int Capacity
    {
        get
        {
            CheckDisposed();
            if (largeBuffer != null) return largeBuffer.Length;

            var size = (long)blocks.Count * memoryManager.BlockSize;
            return (int)Math.Min(int.MaxValue, size);
        }
        set
        {
            CheckDisposed();
            EnsureCapacity(value);
        }
    }

    private int length;

    /// <summary>
    ///     Gets the number of bytes written to this stream.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    public override long Length
    {
        get
        {
            CheckDisposed();
            return length;
        }
    }

    private int position;

    /// <summary>
    ///     Gets the current position in the stream
    /// </summary>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    public override long Position
    {
        get
        {
            CheckDisposed();
            return position;
        }
        set
        {
            CheckDisposed();
            if (value < 0) throw new ArgumentOutOfRangeException("value", "value must be non-negative");

            if (value > MaxStreamLength)
                throw new ArgumentOutOfRangeException("value", "value cannot be more than " + MaxStreamLength);

            position = (int)value;
        }
    }

    /// <summary>
    ///     Whether the stream can currently read
    /// </summary>
    public override bool CanRead => !Disposed;

    /// <summary>
    ///     Whether the stream can currently seek
    /// </summary>
    public override bool CanSeek => !Disposed;

    /// <summary>
    ///     Always false
    /// </summary>
    public override bool CanTimeout => false;

    /// <summary>
    ///     Whether the stream can currently write
    /// </summary>
    public override bool CanWrite => !Disposed;

    /// <summary>
    ///     Returns a single buffer containing the contents of the stream.
    ///     The buffer may be longer than the stream length.
    /// </summary>
    /// <returns>A byte[] buffer</returns>
    /// <remarks>
    ///     IMPORTANT: Doing a Write() after calling GetBuffer() invalidates the buffer. The old buffer is held onto
    ///     until Dispose is called, but the next time GetBuffer() is called, a new buffer from the pool will be required.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
#if NETSTANDARD1_4
        public byte[] GetBuffer()
#else
    public override byte[] GetBuffer()
#endif
    {
        CheckDisposed();

        if (largeBuffer != null) return largeBuffer;

        if (blocks.Count == 1) return blocks[0];

        // Buffer needs to reflect the capacity, not the length, because
        // it's possible that people will manipulate the buffer directly
        // and set the length afterward. Capacity sets the expectation
        // for the size of the buffer.
        var newBuffer = memoryManager.GetLargeBuffer(Capacity, tag);

        // InternalRead will check for existence of largeBuffer, so make sure we
        // don't set it until after we've copied the data.
        InternalRead(newBuffer, 0, length, 0);
        largeBuffer = newBuffer;

        if (blocks.Count > 0 && memoryManager.AggressiveBufferReturn)
        {
            memoryManager.ReturnBlocks(blocks, tag);
            blocks.Clear();
        }

        return largeBuffer;
    }

    /// <summary>
    ///     Returns an ArraySegment that wraps a single buffer containing the contents of the stream.
    /// </summary>
    /// <param name="buffer">An ArraySegment containing a reference to the underlying bytes.</param>
    /// <returns>Always returns true.</returns>
    /// <remarks>
    ///     GetBuffer has no failure modes (it always returns something, even if it's an empty buffer), therefore this method
    ///     always returns a valid ArraySegment to the same buffer returned by GetBuffer.
    /// </remarks>
    public override bool TryGetBuffer(out ArraySegment<byte> buffer)
    {
        CheckDisposed();
        buffer = new ArraySegment<byte>(GetBuffer(), 0, (int)Length);
        // GetBuffer has no failure modes, so this should always succeed
        return true;
    }

    /// <summary>
    ///     Returns a new array with a copy of the buffer's contents. You should almost certainly be using GetBuffer combined
    ///     with the Length to
    ///     access the bytes in this stream. Calling ToArray will destroy the benefits of pooled buffers, but it is included
    ///     for the sake of completeness.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    /// <exception cref="NotSupportedException">The current RecyclableStreamManager object disallows ToArray calls.</exception>
#pragma warning disable CS0809
    [Obsolete("This method has degraded performance vs. GetBuffer and should be avoided.")]
    public override byte[] ToArray()
    {
        CheckDisposed();

        var stack = memoryManager.GenerateCallStacks ? Environment.StackTrace : null;
        RecyclableMemoryStreamManager.Events.Writer.MemoryStreamToArray(id, tag, stack, length);

        if (memoryManager.ThrowExceptionOnToArray)
            throw new NotSupportedException(
                "The underlying RecyclableMemoryStreamManager is configured to not allow calls to ToArray.");

        var newBuffer = new byte[Length];

        InternalRead(newBuffer, 0, length, 0);
        memoryManager.ReportStreamToArray();

        return newBuffer;
    }
#pragma warning restore CS0809

    /// <summary>
    ///     Reads from the current position into the provided buffer
    /// </summary>
    /// <param name="buffer">Destination buffer</param>
    /// <param name="offset">Offset into buffer at which to start placing the read bytes.</param>
    /// <param name="count">Number of bytes to read.</param>
    /// <returns>The number of bytes read</returns>
    /// <exception cref="ArgumentNullException">buffer is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">offset or count is less than 0</exception>
    /// <exception cref="ArgumentException">offset subtracted from the buffer length is less than count</exception>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    public override int Read(byte[] buffer, int offset, int count)
    {
        return SafeRead(buffer, offset, count, ref position);
    }

    /// <summary>
    ///     Reads from the specified position into the provided buffer
    /// </summary>
    /// <param name="buffer">Destination buffer</param>
    /// <param name="offset">Offset into buffer at which to start placing the read bytes.</param>
    /// <param name="count">Number of bytes to read.</param>
    /// <param name="streamPosition">Position in the stream to start reading from</param>
    /// <returns>The number of bytes read</returns>
    /// <exception cref="ArgumentNullException">buffer is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">offset or count is less than 0</exception>
    /// <exception cref="ArgumentException">offset subtracted from the buffer length is less than count</exception>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    public int SafeRead(byte[] buffer, int offset, int count, ref int streamPosition)
    {
        CheckDisposed();
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));

        if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset), "offset cannot be negative");

        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "count cannot be negative");

        if (offset + count > buffer.Length)
            throw new ArgumentException("buffer length must be at least offset + count");

        var amountRead = InternalRead(buffer, offset, count, streamPosition);
        streamPosition += amountRead;
        return amountRead;
    }

#if !NETSTANDARD2_0 && (NETCORE  && NET8_0)
    /// <summary>
    /// Reads from the current position into the provided buffer
    /// </summary>
    /// <param name="buffer">Destination buffer</param>
    /// <returns>The number of bytes read</returns>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    public override int Read(Span<byte> buffer)
    {
        return this.SafeRead(buffer, ref this.position);
    }

    /// <summary>
    /// Reads from the specified position into the provided buffer
    /// </summary>
    /// <param name="buffer">Destination buffer</param>
    /// <param name="streamPosition">Position in the stream to start reading from</param>
    /// <returns>The number of bytes read</returns>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    public int SafeRead(Span<byte> buffer, ref int streamPosition)
    {
        this.CheckDisposed();

        int amountRead = this.InternalRead(buffer, streamPosition);
        streamPosition += amountRead;
        return amountRead;
    }
#endif

    /// <summary>
    ///     Writes the buffer to the stream
    /// </summary>
    /// <param name="buffer">Source buffer</param>
    /// <param name="offset">Start position</param>
    /// <param name="count">Number of bytes to write</param>
    /// <exception cref="ArgumentNullException">buffer is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">offset or count is negative</exception>
    /// <exception cref="ArgumentException">buffer.Length - offset is not less than count</exception>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    public override void Write(byte[] buffer, int offset, int count)
    {
        CheckDisposed();
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));

        if (offset < 0)
            throw new ArgumentOutOfRangeException(nameof(offset), offset,
                "Offset must be in the range of 0 - buffer.Length-1");

        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), count, "count must be non-negative");

        if (count + offset > buffer.Length)
            throw new ArgumentException("count must be greater than buffer.Length - offset");

        var blockSize = memoryManager.BlockSize;
        var end = (long)position + count;
        // Check for overflow
        if (end > MaxStreamLength) throw new IOException("Maximum capacity exceeded");

        EnsureCapacity((int)end);

        if (largeBuffer == null)
        {
            var bytesRemaining = count;
            var bytesWritten = 0;
            var blockAndOffset = GetBlockAndRelativeOffset(position);

            while (bytesRemaining > 0)
            {
                var currentBlock = blocks[blockAndOffset.Block];
                var remainingInBlock = blockSize - blockAndOffset.Offset;
                var amountToWriteInBlock = Math.Min(remainingInBlock, bytesRemaining);

                Buffer.BlockCopy(buffer, offset + bytesWritten, currentBlock, blockAndOffset.Offset,
                    amountToWriteInBlock);

                bytesRemaining -= amountToWriteInBlock;
                bytesWritten += amountToWriteInBlock;

                ++blockAndOffset.Block;
                blockAndOffset.Offset = 0;
            }
        }
        else
        {
            Buffer.BlockCopy(buffer, offset, largeBuffer, position, count);
        }

        position = (int)end;
        length = Math.Max(position, length);
    }

#if !NETSTANDARD2_0 && (NETCORE || NET8_0)
    /// <summary>
    /// Writes the buffer to the stream
    /// </summary>
    /// <param name="source">Source buffer</param>
    /// <exception cref="ArgumentNullException">buffer is null</exception>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    public override void Write(ReadOnlySpan<byte> source)
    {
        this.CheckDisposed();

        int blockSize = this.memoryManager.BlockSize;
        long end = (long)this.position + source.Length;
        // Check for overflow
        if (end > MaxStreamLength)
        {
            throw new IOException("Maximum capacity exceeded");
        }

        this.EnsureCapacity((int)end);

        if (this.largeBuffer == null)
        {
            var blockAndOffset = this.GetBlockAndRelativeOffset(this.position);

            while (source.Length > 0)
            {
                byte[] currentBlock = this.blocks[blockAndOffset.Block];
                int remainingInBlock = blockSize - blockAndOffset.Offset;
                int amountToWriteInBlock = Math.Min(remainingInBlock, source.Length);

                source.Slice(0, amountToWriteInBlock)
                    .CopyTo(currentBlock.AsSpan(blockAndOffset.Offset));

                source = source.Slice(amountToWriteInBlock);

                ++blockAndOffset.Block;
                blockAndOffset.Offset = 0;
            }
        }
        else
        {
            source.CopyTo(this.largeBuffer.AsSpan(this.position));
        }
        this.position = (int)end;
        this.length = Math.Max(this.position, this.length);
    }
#endif

    /// <summary>
    ///     Returns a useful string for debugging. This should not normally be called in actual production code.
    /// </summary>
    public override string ToString()
    {
        return $"Id = {Id}, Tag = {Tag}, Length = {Length:N0} bytes";
    }

    /// <summary>
    ///     Writes a single byte to the current position in the stream.
    /// </summary>
    /// <param name="value">byte value to write</param>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    public override void WriteByte(byte value)
    {
        CheckDisposed();

        var end = (long)position + 1;

        // Check for overflow
        if (end > MaxStreamLength) throw new IOException("Maximum capacity exceeded");

        if (largeBuffer == null)
        {
            var blockSize = memoryManager.BlockSize;

            var block = position / blockSize;

            if (block >= blocks.Count) EnsureCapacity((int)end);

            blocks[block][position % blockSize] = value;
        }
        else
        {
            if (position >= largeBuffer.Length) EnsureCapacity((int)end);

            largeBuffer[position] = value;
        }

        position = (int)end;

        if (position > length) length = position;
    }

    /// <summary>
    ///     Reads a single byte from the current position in the stream.
    /// </summary>
    /// <returns>The byte at the current position, or -1 if the position is at the end of the stream.</returns>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    public override int ReadByte()
    {
        return SafeReadByte(ref position);
    }

    /// <summary>
    ///     Reads a single byte from the specified position in the stream.
    /// </summary>
    /// <param name="streamPosition">The position in the stream to read from</param>
    /// <returns>The byte at the current position, or -1 if the position is at the end of the stream.</returns>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    public int SafeReadByte(ref int streamPosition)
    {
        CheckDisposed();
        if (streamPosition == length) return -1;
        byte value;
        if (largeBuffer == null)
        {
            var blockAndOffset = GetBlockAndRelativeOffset(streamPosition);
            value = blocks[blockAndOffset.Block][blockAndOffset.Offset];
        }
        else
        {
            value = largeBuffer[streamPosition];
        }

        streamPosition++;
        return value;
    }

    /// <summary>
    ///     Sets the length of the stream
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">value is negative or larger than MaxStreamLength</exception>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    public override void SetLength(long value)
    {
        CheckDisposed();
        if (value < 0 || value > MaxStreamLength)
            throw new ArgumentOutOfRangeException(nameof(value),
                "value must be non-negative and at most " + MaxStreamLength);

        EnsureCapacity((int)value);

        length = (int)value;
        if (position > value) position = (int)value;
    }

    /// <summary>
    ///     Sets the position to the offset from the seek location
    /// </summary>
    /// <param name="offset">How many bytes to move</param>
    /// <param name="loc">From where</param>
    /// <returns>The new position</returns>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    /// <exception cref="ArgumentOutOfRangeException">offset is larger than MaxStreamLength</exception>
    /// <exception cref="ArgumentException">Invalid seek origin</exception>
    /// <exception cref="IOException">Attempt to set negative position</exception>
    public override long Seek(long offset, SeekOrigin loc)
    {
        CheckDisposed();
        if (offset > MaxStreamLength)
            throw new ArgumentOutOfRangeException(nameof(offset), "offset cannot be larger than " + MaxStreamLength);

        int newPosition;
        switch (loc)
        {
            case SeekOrigin.Begin:
                newPosition = (int)offset;
                break;
            case SeekOrigin.Current:
                newPosition = (int)offset + position;
                break;
            case SeekOrigin.End:
                newPosition = (int)offset + length;
                break;
            default:
                throw new ArgumentException("Invalid seek origin", nameof(loc));
        }

        if (newPosition < 0) throw new IOException("Seek before beginning");
        position = newPosition;
        return position;
    }

    /// <summary>
    ///     Synchronously writes this stream's bytes to the argument stream.
    /// </summary>
    /// <param name="stream">Destination stream</param>
    /// <remarks>Important: This does a synchronous write, which may not be desired in some situations</remarks>
    /// <exception cref="ArgumentNullException">stream is null</exception>
    public override void WriteTo(Stream stream)
    {
        WriteTo(stream, 0, length);
    }

    /// <summary>
    ///     Synchronously writes this stream's bytes, starting at offset, for count bytes, to the argument stream.
    /// </summary>
    /// <param name="stream">Destination stream</param>
    /// <param name="offset">Offset in source</param>
    /// <param name="count">Number of bytes to write</param>
    /// <exception cref="ArgumentNullException">stream is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Offset is less than 0, or offset + count is beyond  this stream's length.</exception>
    public void WriteTo(Stream stream, int offset, int count)
    {
        CheckDisposed();
        if (stream == null) throw new ArgumentNullException(nameof(stream));

        if (offset < 0 || offset + count > length)
            throw new ArgumentOutOfRangeException(
                "offset must not be negative and offset + count must not exceed the length of the stream",
                innerException: null);

        if (largeBuffer == null)
        {
            var blockAndOffset = GetBlockAndRelativeOffset(offset);
            var bytesRemaining = count;
            var currentBlock = blockAndOffset.Block;
            var currentOffset = blockAndOffset.Offset;

            while (bytesRemaining > 0)
            {
                var amountToCopy = Math.Min(blocks[currentBlock].Length - currentOffset, bytesRemaining);
                stream.Write(blocks[currentBlock], currentOffset, amountToCopy);

                bytesRemaining -= amountToCopy;

                ++currentBlock;
                currentOffset = 0;
            }
        }
        else
        {
            stream.Write(largeBuffer, offset, count);
        }
    }

    #endregion

    #region Helper Methods

    private bool Disposed => Interlocked.Read(ref disposedState) != 0;

    [MethodImpl((MethodImplOptions)256)]
    private void CheckDisposed()
    {
        if (Disposed) ThrowDisposedException();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void ThrowDisposedException()
    {
        throw new ObjectDisposedException($"The stream with Id {id} and Tag {tag} is disposed.");
    }

    private int InternalRead(byte[] buffer, int offset, int count, int fromPosition)
    {
        if (length - fromPosition <= 0) return 0;

        int amountToCopy;

        if (largeBuffer == null)
        {
            var blockAndOffset = GetBlockAndRelativeOffset(fromPosition);
            var bytesWritten = 0;
            var bytesRemaining = Math.Min(count, length - fromPosition);

            while (bytesRemaining > 0)
            {
                amountToCopy = Math.Min(blocks[blockAndOffset.Block].Length - blockAndOffset.Offset,
                    bytesRemaining);
                Buffer.BlockCopy(blocks[blockAndOffset.Block], blockAndOffset.Offset, buffer,
                    bytesWritten + offset, amountToCopy);

                bytesWritten += amountToCopy;
                bytesRemaining -= amountToCopy;

                ++blockAndOffset.Block;
                blockAndOffset.Offset = 0;
            }

            return bytesWritten;
        }

        amountToCopy = Math.Min(count, length - fromPosition);
        Buffer.BlockCopy(largeBuffer, fromPosition, buffer, offset, amountToCopy);
        return amountToCopy;
    }

#if NETCORE 
    private int InternalRead(Span<byte> buffer, int fromPosition)
    {
        if (this.length - fromPosition <= 0)
        {
            return 0;
        }

        int amountToCopy;

        if (this.largeBuffer == null)
        {
            var blockAndOffset = this.GetBlockAndRelativeOffset(fromPosition);
            int bytesWritten = 0;
            int bytesRemaining = Math.Min(buffer.Length, this.length - fromPosition);

            while (bytesRemaining > 0)
            {
                amountToCopy = Math.Min(this.blocks[blockAndOffset.Block].Length - blockAndOffset.Offset,
                    bytesRemaining);
                this.blocks[blockAndOffset.Block].AsSpan(blockAndOffset.Offset, amountToCopy)
                    .CopyTo(buffer.Slice(bytesWritten));

                bytesWritten += amountToCopy;
                bytesRemaining -= amountToCopy;

                ++blockAndOffset.Block;
                blockAndOffset.Offset = 0;
            }
            return bytesWritten;
        }
        amountToCopy = Math.Min(buffer.Length, this.length - fromPosition);
        this.largeBuffer.AsSpan(fromPosition, amountToCopy).CopyTo(buffer);
        return amountToCopy;
    }
#endif

    private struct BlockAndOffset
    {
        public int Block;
        public int Offset;

        public BlockAndOffset(int block, int offset)
        {
            Block = block;
            Offset = offset;
        }
    }

    [MethodImpl((MethodImplOptions)256)]
    private BlockAndOffset GetBlockAndRelativeOffset(int offset)
    {
        var blockSize = memoryManager.BlockSize;
        return new BlockAndOffset(offset / blockSize, offset % blockSize);
    }

    private void EnsureCapacity(int newCapacity)
    {
        if (newCapacity > memoryManager.MaximumStreamCapacity && memoryManager.MaximumStreamCapacity > 0)
        {
            RecyclableMemoryStreamManager.Events.Writer.MemoryStreamOverCapacity(newCapacity,
                memoryManager.MaximumStreamCapacity, tag, AllocationStack);
            throw new InvalidOperationException("Requested capacity is too large: " + newCapacity + ". Limit is " +
                                                memoryManager.MaximumStreamCapacity);
        }

        if (largeBuffer != null)
        {
            if (newCapacity > largeBuffer.Length)
            {
                var newBuffer = memoryManager.GetLargeBuffer(newCapacity, tag);
                InternalRead(newBuffer, 0, length, 0);
                ReleaseLargeBuffer();
                largeBuffer = newBuffer;
            }
        }
        else
        {
            while (Capacity < newCapacity) blocks.Add(memoryManager.GetBlock());
        }
    }

    /// <summary>
    ///     Release the large buffer (either stores it for eventual release or returns it immediately).
    /// </summary>
    private void ReleaseLargeBuffer()
    {
        if (memoryManager.AggressiveBufferReturn)
        {
            memoryManager.ReturnLargeBuffer(largeBuffer, tag);
        }
        else
        {
            if (dirtyBuffers == null)
                // We most likely will only ever need space for one
                dirtyBuffers = new List<byte[]>(1);
            dirtyBuffers.Add(largeBuffer);
        }

        largeBuffer = null;
    }

    #endregion
}
