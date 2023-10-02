using System;
using System.Threading;


// Пример ниже показывает решение, где вилки не представляются явно.
//  Философы могут есть, если ни один из их соседей не ест.
//   Аналогично системе, где философы, которые не могут взять вторую вилку, должны положить первую вилку до того, как они попробуют снова.

// В отсутствие блокировок, связанных с вилками, философы должны обеспечивать то, что начало принятия пищи не основывается на старой информации о состоянии соседей.
//  Например: Если философ Б видит, что A не ест в данный момент времени, а потом поворачивается и смотрит на В, 
//  A мог начать есть, пока философ Б смотрит на В. 
//  Используя одну взаимоисключающую блокировку (Мьютекс), можно избежать этой проблемы.
//   Эта блокировка не связана с вилками, но она связана с решением процедур, которые могут изменить состояние философов. Это обеспечивается монитором.

// Алгоритм монитора реализует схему «проверить, взять и положить» и совместно использует взаимоисключающую блокировку. Заметьте, что философы, желающие есть, не будут иметь вилок.

// Если монитор разрешает философу, желающему есть, действовать, то философ снова завладевает первой вилкой, прежде чем взять уже свободную вторую.

// По окончании текущего приёма пищи философ оповещает монитор о том, что обе вилки свободны.

// Стоит заметить, что этот алгоритм монитора не решает проблемы голодания. 
// Например, философ Б может бесконечно ждать своей очереди, если у философов A и В периоды приёма пищи всё время пересекаются. 
// Чтобы гарантировать также, что ни один философ не будет голодать, можно отслеживать, сколько раз голодный философ не ел, когда его соседи положили вилки на стол.
//  Если количество раз превысит некий предел, такой философ перейдёт в состояние
//   Голодания и алгоритм монитора форсирует процедуру завладения вилками, выполняя условие недопущения голодания ни одного из соседей.

// Философ, не имеющий возможности взять вилки из-за того, что его сосед голодает, находится в режиме полезного ожидания окончания приёма пищи соседом его соседа. 
// Эта дополнительная зависимость снижает параллелизм. Увеличение значения порога перехода в состояние Голодание уменьшает этот эффект. 
class DiningPhilosophers
{
    static Random random = new Random();

    static void Main()
    {
        Console.WriteLine("Dining Philosophers C# with Resource hierarchy");

        DiningPhilosophers dining = new DiningPhilosophers(5);
        dining.Start();

        Console.ReadLine();
    }

    public DiningPhilosophers(int count)
    {
        sereal = 0;
        m = new Mutex();

        self = new object[count];
        for (int i = 0; i < 5; i++)
        {
            self[i] = new object();
        }

        state = new PhilosopherState[count];
        for (int i = 0; i < 5; i++)
        {
            state[i] = PhilosopherState.Thinking;
        }
    }

    public void Start()
    {
        Thread[] philosopherThreads = new Thread[5];

        for (int i = 0; i < 5; i++)
        {
            philosopherThreads[i] = new Thread(Task);
            philosopherThreads[i].Start();
        }

        foreach (var thread in philosopherThreads)
        {
            thread.Join();
        }
    }

    public void Task()
    {
        int i = sereal;
        sereal++;
        while (true)
        {
            Think(i);
            Pickup(i);
            Eat(i);
            Putdown(i);
        }
    }


    private int sereal;
    private Mutex m;
    private object[] self;
    private PhilosopherState[] state;

    private enum PhilosopherState
    {
        Thinking,
        Hungry,
        Eating
    }

    private void Think(int id)
    {
        int duration = random.Next(200, 800);
        Console.WriteLine($"{id} thinks for {duration}ms");
        Thread.Sleep(duration);
    }

    public void Pickup(int i)
    {
        bool shouldSlepp = false;

        m.WaitOne();
        {
            state[i] = PhilosopherState.Hungry;
            Console.WriteLine($"\t\t{i} is hungry");
            Test(i);

            if (state[i] != PhilosopherState.Eating)
            {
                shouldSlepp = true;
            }
        }
        m.ReleaseMutex();

        if (shouldSlepp)
        {
            lock (self[i])
            {
                Monitor.Wait(self[i]);
            }
        }
    }

    private void Eat(int id)
    {
        int duration = random.Next(200, 800);
        Console.WriteLine($"\t\t\t\t{id} eats for {duration}ms");
        Thread.Sleep(duration);
    }

    public void Putdown(int i)
    {
        m.WaitOne();
        {
            state[i] = PhilosopherState.Thinking;
            Test((i + 5 - 1) % 5);
            Test((i + 1) % 5);
        }
        m.ReleaseMutex();
    }

    private void Test(int i)
    {
        if (state[(i + 5 - 1) % 5] != PhilosopherState.Eating
            && state[(i + 1) % 5] != PhilosopherState.Eating
            && state[i] == PhilosopherState.Hungry)
        {
            state[i] = PhilosopherState.Eating;
            lock (self[i])
            {
                Monitor.Pulse(self[i]);
            }
        }
    }
}