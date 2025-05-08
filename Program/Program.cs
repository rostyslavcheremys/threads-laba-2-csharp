namespace Program;

abstract class Program
{
    private static int[]? _array;
    
    private static int _minElement;
    private static int _minIndex;
    private static int _completedThreads;
    
    private static readonly Random Random = new();
    private static readonly Lock LockObject = new();
    private static readonly object CompletionLock = new();

    static void Main()
    {
        int[] threadCounts = [4];
        
        foreach (int threadCount in threadCounts)
        {
            Console.WriteLine($"\nRunning with {threadCount} threads:\n");
            FindMinElement(threadCount);
            
            lock (CompletionLock)
            {
                while (_completedThreads < threadCount)
                {
                    Monitor.Wait(CompletionLock);
                }
            }
        }
    }

    static void FindMinElement(int threadCount)
    {
        int arraySize = 10000000;
        _array = new int[arraySize];
        
        for (int i = 0; i < arraySize; i++)
        {
            _array[i] = Random.Next(1, 1000000);
        }
        
        int randomIndex = Random.Next(arraySize);
        _array[randomIndex] = -Random.Next(1, 1000);
        
        _minElement = int.MaxValue;
        _minIndex = -1;
        _completedThreads = 0;
        
        Thread[] threads = new Thread[threadCount];
        
        int chunkSize = arraySize / threadCount;
        
        for (int i = 0; i < threadCount; i++)
        {
            int startIndex = i * chunkSize;
            int endIndex = (i == threadCount - 1) ? arraySize : startIndex + chunkSize;
            
            threads[i] = new Thread(() => FindMinInRange(startIndex, endIndex, threadCount));
            threads[i].Start();
        }
    }

    static void FindMinInRange(int startIndex, int endIndex, int threadCount)
    {
        int localMin = int.MaxValue;
        int localMinIndex = -1;
        
        for (int i = startIndex; i < endIndex; i++)
        {
            if (_array![i] < localMin)
            {
                localMin = _array[i];
                localMinIndex = i;
            }
        }
        
        lock (LockObject)
        {
            if (localMin < _minElement)
            {
                _minElement = localMin;
                _minIndex = localMinIndex;
            }
        }
        
        lock (CompletionLock)
        {
            _completedThreads++;
            Monitor.Pulse(CompletionLock); 
            
            if (_completedThreads == threadCount)
            {
                Console.WriteLine($"Minimum element: {_minElement}");
                Console.WriteLine($"Index of minimum element: {_minIndex}");
            }
        }
    }
}