using System.Collections;

namespace Rin.Core.Collections;

/// <summary>
///     A pool of objects allocated and can be cleared without loosing previously allocated instance.
/// </summary>
/// <typeparam name="T">Type of the pooled object</typeparam>
public struct PoolListStruct<T> : IEnumerable<T> where T : class {
    /// <summary>
    ///     The list of allocated objects.
    /// </summary>
    FastListStruct<T> allocated;

    /// <summary>
    ///     A factory to allocate new objects.
    /// </summary>
    readonly Func<T> factory;

    /// <summary>
    ///     The number of objects in use, readonly.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    ///     Gets or sets the item at the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>An instance of T</returns>
    public T this[int index] {
        get => allocated.Items[index];
        set => allocated.Items[index] = value;
    }

    /// <summary>
    ///     Gets or sets the item at the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>An instance of T</returns>
    public T this[uint index] {
        get => allocated.Items[index];
        set => allocated.Items[index] = value;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PoolListStruct{T}" /> struct.
    /// </summary>
    /// <param name="capacity">The capacity.</param>
    /// <param name="factory">The factory.</param>
    /// <exception cref="System.ArgumentNullException">factory</exception>
    public PoolListStruct(int capacity, Func<T> factory) {
        this.factory = factory;
        allocated = new(capacity);
        Count = 0;
    }

    /// <summary>
    ///     Clears objects in use and keep allocated objects.
    /// </summary>
    public void Clear() {
        Count = 0;
    }

    /// <summary>
    ///     Resets this instance by releasing allocated objects.
    /// </summary>
    public void Reset() {
        Clear();
        for (var i = 0; i < allocated.Count; i++) {
            allocated[i] = null;
        }

        allocated.Clear();
    }

    /// <summary>
    ///     Adds a new object in use to this instance.
    /// </summary>
    /// <returns>An instance of T</returns>
    public T Add() {
        T result;
        if (Count < allocated.Count) {
            result = allocated[Count];
        } else {
            result = factory();
            allocated.Add(result);
        }

        Count++;
        return result;
    }

    /// <summary>
    ///     Gets the index of <paramref name="item" />
    /// </summary>
    /// <param name="item">The item to get the index of</param>
    /// <returns>Index of the item, or -1 if the item is not in this list</returns>
    public int IndexOf(T item) => allocated.IndexOf(item);

    public void RemoveAt(int index) {
        if (index < 0 || index >= Count) {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        Count--;
        var oldItem = allocated[index];

        // This will shift all items after this item 1 to the left but without changing the capacity of the container
        allocated.RemoveAt(index);

        // Place item at the end
        allocated.Add(oldItem);
    }

    /// <summary>
    ///     Removes the object from the list
    /// </summary>
    /// <param name="item">The item to remove</param>
    /// <remarks>The item is added back in the pool to be reused for the next <see cref="Add" /></remarks>
    public void Remove(T item) {
        var removeIndex = IndexOf(item);
        if (removeIndex == -1) {
            throw new InvalidOperationException();
        }

        RemoveAt(removeIndex);
    }

    public FastListStruct<T>.Enumerator GetEnumerator() => new(allocated.Items, Count);
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => new FastListStruct<T>.Enumerator(allocated.Items, Count);
    IEnumerator IEnumerable.GetEnumerator() => new FastListStruct<T>.Enumerator(allocated.Items, Count);
}
