using System;
using System.Threading;

// Описание принципа работы взял с https://ru.wikipedia.org/wiki/%D0%97%D0%B0%D0%B4%D0%B0%D1%87%D0%B0_%D0%BE%D0%B1_%D0%BE%D0%B1%D0%B5%D0%B4%D0%B0%D1%8E%D1%89%D0%B8%D1%85_%D1%84%D0%B8%D0%BB%D0%BE%D1%81%D0%BE%D1%84%D0%B0%D1%85
// Решение достигается путём присвоения частичного порядка ресурсам (в данном случае вилкам) и
//  установления соглашения, что ресурсы запрашиваются в указанном порядке, а возвращаются в обратном порядке. 
//  Кроме того, не должно быть двух ресурсов, не связанных порядком, используемых одной рабочей единицей.

// Пусть ресурсы(вилки) будут пронумерованы от 1 до 5, и каждая рабочая единица (философ) всегда 
// берёт сначала вилку с наименьшим номером, а потом вилку с наибольшим номером из двух доступных. 
// Далее, философ кладёт сначала вилку с бо́льшим номером, потом — с меньшим. В этом случае, если четыре из
//  пяти философов одновременно возьмут вилку с наименьшим номером, на столе останется вилка с наибольшим возможным номером. 
//  Таким образом, пятый философ не сможет взять ни одной вилки. Более того, только один философ будет иметь доступ к вилке с наибольшим номером,
//   так что он сможет есть двумя вилками. Когда он закончит использовать вилки, он в первую очередь положит
//    на стол вилку с бо́льшим номером, потом — с меньшим, тем самым позволив другому философу взять недостающую вилку и приступить к еде.

// В то время, как иерархия ресурсов позволяет избежать взаимных блокировок, данное решение не всегда является практичным,
//  в особенности когда список необходимых ресурсов неизвестен заранее.
//   Например, если рабочая единица удерживает ресурс 3 и 5 и решает, что ей необходим ресурс 2,
//    то она должна выпустить ресурс 5, затем 3, после этого завладеть ресурсом 2 и снова взять ресурс 3 и 5.
//     Компьютерные программы, которые работают с большим количеством записей в базе данных, 
//     не смогут работать эффективно, если им потребуется выпускать все записи с верхними индексами прежде,
//      чем завладеть новой записью. Это делает данный метод непрактичным. 

class DiningPhilosophers
{
    static Random random = new Random();

    static void Main()
    {
        Console.WriteLine("Dining Philosophers C# with Resource hierarchy");

        int numPhilosophers = 5; // Измените это значение на требуемое количество философов

        DiningTable table = new DiningTable(numPhilosophers);
        table.StartDining();

        Console.ReadLine();
    }

    // DiningTable - адаптивная система, для удобного изменения количества философов.
    class DiningTable
    {
        private Fork[] forks;
        private Philosopher[] philosophers;

        public DiningTable(int numPhilosophers)
        {
            forks = new Fork[numPhilosophers];
            philosophers = new Philosopher[numPhilosophers];

            for (int i = 0; i < numPhilosophers; i++)
            {
                forks[i] = new Fork();
            }

            for (int i = 0; i < numPhilosophers; i++)
            {
                int leftForkIndex = i;
                int rightForkIndex = (i + 1) % numPhilosophers;

                if (i != numPhilosophers - 1)
                {
                    philosophers[i] = new Philosopher(i + 1, forks[leftForkIndex], forks[rightForkIndex]);
                }
                else
                {
                    philosophers[i] = new Philosopher(i + 1, forks[rightForkIndex], forks[leftForkIndex]);
                }
            }
        }

        public void StartDining()
        {
            Thread[] philosopherThreads = new Thread[philosophers.Length];

            for (int i = 0; i < philosophers.Length; i++)
            {
                philosopherThreads[i] = new Thread(philosophers[i].StartDining);
                philosopherThreads[i].Start();
            }

            foreach (var thread in philosopherThreads)
            {
                thread.Join();
            }
        }
    }

    class Fork
    {
        private readonly object lockObject = new object();
        public void PickUp()
        {
            Monitor.Enter(lockObject);
        }

        public void PutDown()
        {
            Monitor.Exit(lockObject);
        }
    }

    class Philosopher
    {
        private readonly int id;
        private readonly Fork leftFork;
        private readonly Fork rightFork;

        public Philosopher(int id, Fork leftFork, Fork rightFork)
        {
            this.id = id;
            this.leftFork = leftFork;
            this.rightFork = rightFork;
        }

        public void StartDining()
        {
            while (true)
            {
                Think();
                PickUpForks();
                Eat();
                PutDownForks();
            }
        }

        private void Think()
        {
            int duration = random.Next(200, 800);
            Console.WriteLine($"{id} thinks for {duration}ms");
            Thread.Sleep(duration);
        }

        private void PickUpForks()
        {
            Console.WriteLine($"\t\t{id} is hungry");
            lock (leftFork)
            {
                lock (rightFork)
                {
                }
            }
        }

        private void Eat()
        {
            int duration = random.Next(200, 800);
            Console.WriteLine($"\t\t\t\t{id} eats for {duration}ms");
            Thread.Sleep(duration);
        }

        private void PutDownForks()
        {
            lock (rightFork)
            {
                lock (leftFork)
                {
                }
            }
        }
    }
}
