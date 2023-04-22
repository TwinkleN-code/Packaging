using System;
using Packaging;

/// THIS IS STUDENT SOLUTION
/// 
/// Concurrent version of the Packaging
namespace ConcPackaging;

public class ConcWorker : Worker
{
    // todo: add required properties for a thread-safe concurrent worker
    public string Name;
    public int NumMoves;
    public ConcStorage concStorage;

    public ConcWorker(string n, ConcStorage s) : base(n,s)
    {
        // todo: implement the body
        this.Name = n;
        this.NumMoves = 0;
        this.concStorage = s;
    }

    //todo: add required methods to implement a thread-safe concurrent worker
    public override void PackItems()
    {
        Item cur_item;
        while (true)
        {
            lock(concStorage.Items)
            {
                // grab and remove the first item
                if (concStorage.Items.Count > 0)
                {
                    cur_item = concStorage.Items.First.Value;
                    concStorage.num_pickedItems++;
                    concStorage.Items.RemoveFirst();
                }
                else
                {
                    return;
                }              
            }
            
            // move the item to the boxes
            Move();

            lock (concStorage.Boxes)
            {
                // find the corresponding box and pack it
                foreach (Box b in concStorage.Boxes)
                {
                    if (cur_item.BoxName == b.Name)
                    {
                        b.AddItem(cur_item);
                        concStorage.num_packedItems++;
                        this.Log(cur_item.Name + " moved " + b.Name);
                        break;
                    }
                }
            }            
        }
    }
}

public class ConcStorage : Storage
{
    // todo: add required properties for a thread-safe concurrent storage
    public ConcWorker ConcWorker;
    public LinkedList<Box> Boxes;
    public LinkedList<Item> Items;
    public int num_pickedItems, num_packedItems;


    public ConcStorage()
    {
        this.Boxes = new LinkedList<Box>();
        this.Items = new LinkedList<Item>();
        num_packedItems = 0;
        num_pickedItems = 0; 
    }

    public override void Initialize()
    {
        for (int i = 0; i < FixedParams.maxNumOfBoxes; i++)
            this.Boxes.AddLast(new Box("BOX_" + (i).ToString()));

        for (int i = 0; i < FixedParams.maxNumOfItems; i++)
            this.Items.AddLast(new Item("ITM_" + (i).ToString()));
    }

    public override void Assign()
    {
        lock (this.Items)
        {
            foreach (Item i in this.Items)
                i.AssignBox("BOX_" + new Random().Next(0, FixedParams.maxNumOfBoxes).ToString());
        }
    }

    public override void StartPackaging()
    {
        // todo: replace the exception with your implementation of the body

        //var thread = new Thread(() => { });

        //for (int i = 1; i <= FixedParams.maxNumOfWorkers; i++)
        //{
        //    var worker = new ConcWorker("WOR_" + (i).ToString(), this);
        //    thread = new Thread(worker.PackItems);
        //    thread.Start();
        //}

        //thread.Join();

        Thread[] WorkerThreads = new Thread[FixedParams.maxNumOfWorkers];
        for (int j = 0; j < FixedParams.maxNumOfWorkers; j++)
        {
            var worker = new ConcWorker("WOR_" + (j + 1).ToString(), this);
            WorkerThreads[j] = new Thread(worker.PackItems);
        }

        for (int i = 0; i < FixedParams.maxNumOfWorkers; i++)
        {
            WorkerThreads[i].Start();
        }

        for (int i = 0; i < FixedParams.maxNumOfItems; i++)
        {
            WorkerThreads[1].Join();
        }

    }

    // todo: add required properties for a thread-safe concurrent storage
    public override Statistics GetStatistics()
    {
        int totalNumOfItems = 0;
        bool correctPacking = true;
   
        foreach (Box b in this.Boxes)
        {
            totalNumOfItems += b.Items.Count;
            foreach (Item i in b.Items)
                correctPacking = correctPacking && (i.BoxName == b.Name);
        }

        Statistics stats = new Statistics();
        stats.TotalNumItems = totalNumOfItems;
        stats.TotalNumBoxes = Boxes.Count;
        stats.TotalNumPicks = num_pickedItems;
        stats.TotalNumPacked = num_packedItems;
        stats.AllBoxesAreCorrect = correctPacking;

        return stats;
    }

}

/// <summary>
/// A class to run the packaging concurrently.
/// </summary>
public class PackagingConcurrent
{
    private ConcStorage _conc_storage;

    public PackagingConcurrent()
    {
        this._conc_storage = new();
    }
    public void RunPackaging()
    {
        _conc_storage.Initialize();
        _conc_storage.Assign();
        _conc_storage.StartPackaging();
    }
    public Statistics FinalResult()
    {
        return _conc_storage.GetStatistics();
    }
}