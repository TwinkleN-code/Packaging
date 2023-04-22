using System;
using System.IO;
using System.Diagnostics;
using ConcPackaging;
//using Solution; // This must stay commented for the students

namespace Packaging;

// The values within SubmissionParams must be complete in the submission. 
class SubmissionParams
{
    public const string studentNumberOne = "0993320"; // This must be filled.
    public const string studentNumberTwo = "1005210"; // This must be filled. Keep it "" if you are working alone.
    public const string classNumber = "INF2C"; // This must be filled. INF2A is just an example.
}

// The values of FixedParams must not change in the final submission.
// 
class FixedParams
{
    public const int minWorkingTime = 10;
    public const int maxWorkingTime = 20;
    public const int maxNumOfItems = 5000; 
    public const int maxNumOfBoxes = 50; 
    public const int maxNumOfWorkers = 1000; //10
    public const char delim = ',';
    public const bool verbose = true; // controls if intermediate log messages to be printed during the execution
}

class Program
{
    static void Main(string[] args)
    {
        string logFooter = "", logSeqContent = " Sequential Run: \n", logConcContent = " Concurrent Run: \n", logTiming = "";

        Stopwatch seqSW = new Stopwatch();
        Stopwatch conSW = new Stopwatch();

        seqSW.Start();
        Console.WriteLine("Sequential packaging is going to start ...");
        PackagingSequential sp = new();
        sp.RunPackaging();
        logSeqContent = logSeqContent + sp.FinalResult();
        seqSW.Stop();

        TimeSpan seqET = seqSW.Elapsed;

        conSW.Start();
        Console.WriteLine("Concurrent packaging is going to start ...");
        PackagingConcurrent cp = new();
        cp.RunPackaging();
        logConcContent = logConcContent + cp.FinalResult().ToString();
        conSW.Stop();


        TimeSpan conET = conSW.Elapsed;

        logTiming =
            "Time Sequential = " + seqET.Minutes + " min, " + seqET.Seconds + "sec, " + seqET.Milliseconds + " msec. " + "\n" +
            "Time Concurrent = " + conET.Minutes + " min, " + conET.Seconds + "sec, " + conET.Milliseconds + " msec. " + "\n";

        logFooter =
            "Number of Items: " + FixedParams.maxNumOfItems + "\n" +
            "Number of Boxes: " + FixedParams.maxNumOfBoxes + "\n" +
            "Number of Workers: " + FixedParams.maxNumOfWorkers + "\n" +
            "Class: " + SubmissionParams.classNumber + "\n" +
            "Student Number One: " + SubmissionParams.studentNumberOne + "\n" +
            "Student Number Two: " + SubmissionParams.studentNumberTwo + "\n";

        try
        {
            //var basePath = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            //logFilePath = basePath.Parent.Parent.Parent.FullName;
            //logContent = logResult + logTiming + logFooter;
            //System.IO.File.WriteAllText(logFilePath + FixedParams.logFileName, logContent);
        }
        catch (Exception e) { Console.WriteLine(e.ToString()); }

        Console.WriteLine("----------------");
        Console.WriteLine(logSeqContent);
        Console.WriteLine("----------------");
        Console.WriteLine(logConcContent);
        Console.WriteLine("----------------");
        Console.WriteLine(logFooter);
        Console.WriteLine(logTiming);


        /** NOTE:
         * In concurrent programs it is very difficult to reproduce the errors.
         * For your own experiments, in this assignment, you can uncomment the follwoing method call and see if multiple executions have correct results. 
         * In order to see the intermediate summary of print outs, in FixedParams put "verbose" as "false".
         * Keep in mind this method DOES NOT measure the PERFORMANCE (timing) of your solution. It only checks the correctnes of object fields.
         * **/
        //Program.EvaluateConc();

    }

    static void EvaluateConc(int maxIter = 10)
    {
        bool result = true;
        Console.WriteLine("Start of the experiments for concurrent version...");
        // begin of the loop
        for (int iter = 1; iter <= maxIter; iter++)
        {
            PackagingConcurrent cp = new();
            cp.RunPackaging();
            result = result && cp.FinalResult().IsCorrect();
            Console.WriteLine("----------------");
            Console.WriteLine("Experiment #" + iter.ToString() + " was " + (cp.FinalResult().IsCorrect() ? "Correct" :"INCORRECT"));
            Console.WriteLine(cp.FinalResult().ToString());
        }
        if (result)
            Console.WriteLine("CORRECT: All experiments produced correct results.");
        else
            Console.WriteLine("NOT CORRECT: Some experiments produced incorrect results.");
    }
}