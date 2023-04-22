using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
//using Solution;

namespace Packaging;

/// <summary>
/// This class defines the structure of an Item.
/// </summary>
public class Item
{
    public string? BoxName, Name;
    /// <summary>
    /// Constructror
    /// </summary>
    /// <param name="txt"> The text for the name of the item.</param>
    public Item(string n = "") => this.Name = n;
    /// <summary>
    /// Specifies the box that must contain this item.
    /// </summary>
    /// <param name="b">Containing box</param>
    public void AssignBox(string b) => this.BoxName = b;
    /// <summary>
    /// Prepares a string format of this object to be logged.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => "Item to be in:"+this.BoxName;
}
/// <summary>
/// Box to be used as the container for items.
/// </summary>
public class Box
{
    public string Name;
    public LinkedList<Item> Items = new();     // Each box can collect some items.

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="txt"> The name of the box. </param>
    public Box(string n) => this.Name = n;

    /// <summary>
    /// Adds an item to the box.
    /// </summary>
    /// <param name="i"> The item will be added to the box. </param>
    public virtual void AddItem(Item i) => this.Items.AddLast(i);

    /// <summary>
    /// Prepares a string format of this object to be logged.
    /// </summary>
    /// <returns></returns>
    public override String ToString() => "Box " + this.Name +" with " + this.Items.Count.ToString() + " items.";

    /// <summary>
    /// Logs the actions, if the verbose is set true.
    /// </summary>
    /// <param name="logText">An explanatory text of the log</param>
    public virtual void Log(string logText = "")
    {
        string nl = "\n";
        if (Packaging.FixedParams.verbose)
            Console.WriteLine(this.ToString() + nl + logText);
    }
}
/// <summary>
/// Defines the class for Worker to move items to the corresponding boxes.
/// </summary>
public class Worker
{
    public string Name;
    public int NumMoves;
    public Storage storage;

    public Worker(string n, Storage s)
    {
        this.NumMoves = 0;
        this.storage = s;
        this.Name = n;
    }

    /// <summary>
    /// The worker starts packing the items: picks the first item, finds corresponding box, removes from the list of items and adds to the box.
    /// </summary>
    public virtual void PackItems()
    {
        Item cur_item;
        while (true)
        {
            // grab and remove the first item
            if (storage.Items.Count > 0)
            {
                cur_item = storage.Items.First.Value;
                storage.num_pickedItems++;
                storage.Items.RemoveFirst(); 
            }
            else
                return;
            // move the item to the boxes
            Move();

            // find the corresponding box and pack it
            foreach (Box b in storage.Boxes)
                if (cur_item.BoxName == b.Name)
                {
                    b.AddItem(cur_item);
                    storage.num_packedItems++;
                    this.Log(cur_item.Name + " moved " + b.Name);
                    break;
                }
        }
    }

    /// <summary>
    /// Moving process for the workers.
    /// </summary>
    public virtual void Move() => Thread.Sleep(new Random().Next(FixedParams.minWorkingTime, FixedParams.maxWorkingTime));

    public override string ToString() => "[Worker] " + this.Name;

    /// <summary>
    /// Logs the actions, if the verbose is set true.
    /// </summary>
    /// <param name="logText">An explanatory text of the log</param>
    public virtual void Log(string logText = "")
    {
        string nl = "\n";
        if (Packaging.FixedParams.verbose)
            Console.WriteLine(this.ToString() + nl + logText);
    }

}

/// <summary>
/// Defines the storage in which items are packed within boxes.
/// </summary>
public class Storage
{
    public Worker Worker;
    public LinkedList<Box> Boxes;
    public LinkedList<Item> Items;
    public int num_pickedItems, num_packedItems;

    public Storage()
    {
        this.Worker = new("WOR_1",this);
        this.Boxes = new LinkedList<Box>();
        this.Items = new LinkedList<Item>();
        num_packedItems = 0;
        num_pickedItems = 0;
    }

    /// <summary>
    /// Sets up the items, boxes, worker(s) and storage.
    /// </summary>
    public virtual void Initialize()
    {
        for (int i = 0; i < FixedParams.maxNumOfBoxes; i++)
            this.Boxes.AddLast(new Box("BOX_"+(i).ToString()));

        for (int i = 0; i< FixedParams.maxNumOfItems; i++)
            this.Items.AddLast(new Item("ITM_"+(i).ToString()));
    }

    /// <summary>
    /// For each item assigns a box name to be packed in by the worker(s).
    /// </summary>
    public virtual void Assign()
    {
        foreach (Item i in this.Items)
            i.AssignBox("BOX_"+new Random().Next(0, FixedParams.maxNumOfBoxes).ToString());
    }

    /// <summary>
    /// Ask worker(s) to pack items.
    /// </summary>
    public virtual void StartPackaging()
    {
        Worker.PackItems();
    }

    // <summary>
    // Prepares some information about the packaging in the storage
    // </summary>
    // <returns>Statistical results as an object of Statistics</returns>
    public virtual Statistics GetStatistics()
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
/// A class to run the packaging sequentially.
/// </summary>
public class PackagingSequential
{
    Storage storage;

    public PackagingSequential()
    {
        this.storage = new Storage();
    }
    public void RunPackaging()
    {
        storage.Initialize();
        storage.Assign();
        storage.StartPackaging();
    }
    public Statistics FinalResult()
    {
        return storage.GetStatistics();
    }
}

public class Statistics
{
    public int TotalNumWorkers = 0, TotalNumItems = 0, TotalNumBoxes = 0, TotalNumPicks = 0, TotalNumPacked = 0;
    public bool AllBoxesAreCorrect = false;

    public bool IsCorrect() => (TotalNumItems == (FixedParams.maxNumOfItems) && TotalNumItems == TotalNumPicks && TotalNumPicks == TotalNumPacked && AllBoxesAreCorrect);
    public override string ToString()
    {
        string result = "", nl = "\n";
        result = "#Items: " + this.TotalNumItems.ToString() + nl +
            "#Picks: " + this.TotalNumPicks.ToString() + nl +
            "#Packed: " + this.TotalNumPacked.ToString() + nl +
            "#Boxes: " + this.TotalNumBoxes.ToString() + nl;
        return result;

    }
}

