class ConcurrentCircularBuffer<T>
{
    private int _capacity;
    private int _currentPosition;
    private T[] _elements;
    private readonly object _lock = new object();

    public ConcurrentCircularBuffer(int capacity,T[] elements)
    {
        this._capacity = capacity;
        this._elements = elements;
    }

    public T GetNext()
    {
        lock (_lock)
        {
            if (_currentPosition < _capacity - 1)
            {
                _currentPosition++;
                return _elements[_currentPosition];
            }

            _currentPosition = 0;
            return _elements[0];
        }
    }
}

