
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.FileIO;
using NgramAnalysis.Models;

#region Variables

var rows = new List<string>();
List<List<string>> newContent = new List<List<string>>(1000000);
List<string> twoLetterGroups = new List<string>();
List<string> nGrams= new List<string>();
List<string> words = new List<string>();
List<string> checkList = new List<string>();
var threadList = new List<ThreadModel>();

ReadTxt();

int threadRange = rows.Count / 10;
int remaningProcess = rows.Count() - (threadRange * 10);
var threads = new List<Thread>();
int startPoint = 0;
int maxRange = 0;

#endregion



for(int i=0;i<10;i++)
{
    maxRange = maxRange + threadRange;
    threadList.Add(new ThreadModel()
    {
        StartPoint = startPoint,
        MaxRange = maxRange,
        ThreadNumber = i
    });
  
    startPoint = startPoint + threadRange;
}

for(int j=0;j<rows.Count;j++)
{
    rows[j] = Regex.Replace(rows[j], "[^\\w ]+", "", RegexOptions.Compiled);
}

foreach (var thread in threadList)
{
    Thread thd = new Thread(() => {
        WorkerDoWork(thread);
    });
    
    thd.Start();
    thd.Join();
}

if (remaningProcess > 0)
{
    var remainingRows = checkList.Skip(rows.Count() - remaningProcess).Take(remaningProcess).ToList();
    var masterThreadRows = new ThreadModel()
    {
        StartPoint = rows.Count() - remaningProcess,
        MaxRange = rows.Count()
    };
    var masterThread = new Thread(() =>
    {
        WorkerDoWork(masterThreadRows);

    });
    
    masterThread.Start();
    masterThread.Join();
}

var newWordList = new List<List<List<string>>>();

for (int i=0;i<newContent.Count;i++)
{
    var wordList = new List<List<string>>();
    for (int j=0;j<newContent[i].Count;j++)
    {
        List<string> kelime = new List<string>();
        kelime = newContent[i][j].Split().Select(x => x.TrimEnd(",.;:-".ToCharArray()).ToLower()).ToList();
        wordList.Add(kelime);
    }
    
    newWordList.Add(wordList);
}

TwoNGramFrequency(newWordList);

using (StreamWriter outPutWriter = new StreamWriter("out_20195156024.txt"))
{
    outPutWriter.WriteLine("2gram Word - Frequency ");
    for (int z=0;z<nGrams.Count();z+=2)
    {
        outPutWriter.WriteLine(nGrams[z]+" : "+nGrams[z+1]);
    }
}

Console.ReadLine();

void TwoNGramFrequency(List<List<List<string>>> allWords)
{
    foreach (var word in allWords)
    {
        for(int j=0;j<word.Count;j++)
        {
            for(int t =0;t<word[j].Count;t++)
            {
                words.Add(word[j][t]);
            }
        }
    }
  
    for(int i= 0;i<words.Count-1;i++)
    {
        twoLetterGroups.Add(words[i] +" "+ words[i + 1]);
    }

    checkList = twoLetterGroups.Distinct().ToList();
    
    var threadList = new List<ThreadModel>();
    int threadRange = checkList.Count / 10;
    int remainingProcess = checkList.Count() - (threadRange * 10);;
    int startPoint = 0;
    int maxRange = 0;
    
    for (int i = 0; i < 10; i++)
    {
        maxRange = maxRange + threadRange;
        threadList.Add(new ThreadModel()
        {
            StartPoint = startPoint,
            MaxRange = maxRange,
            ThreadNumber = 1
        });
        
        startPoint = startPoint + threadRange;
    }
    
    foreach (var thread in threadList)
    {
        Thread thd = new Thread(() =>
        {
            FrequencyDoWork(checkList.Skip(thread.StartPoint).Take(thread.MaxRange - thread.StartPoint).ToList());
        });
        
        thd.Start();
        thd.Join();
    }
    
    if(remainingProcess > 0)
    {
        List<string> remaningCheckList = checkList.Skip(checkList.Count() - remainingProcess).Take(remainingProcess).ToList();
        Thread thdMaster = new Thread(() =>
        {
            FrequencyDoWork(remaningCheckList);

        });
        
        thdMaster.Start();
        thdMaster.Join();
    }
}

void FrequencyDoWork(List<string> group)
{
   var hash = new HashSet<string>(group);
   var result = twoLetterGroups
                                    .Where(s => hash.Contains(s))
                                    .GroupBy(s => s)
                                    .Select(g => (g.Key, count: g.Count()));
   
   foreach (var item in result)
   {
      nGrams.Add(item.Key);
      nGrams.Add(item.count.ToString());
   }
}

void WorkerDoWork(ThreadModel thread)
{

   var newRows = new List<string>();
   for (int i = thread.StartPoint; i < thread.MaxRange; i++)
   {
      newRows.Add(rows[i]);
      
   }
   
   newContent.Add(newRows);
}

void ReadTxt()
{
   using (var reader = new StreamReader("news.txt"))
   {
      string line;
      while ((line = reader.ReadLine()) != null)
      {
         if (!string.IsNullOrWhiteSpace(line))
         {
            rows.Add(line);
         }
      }
   }
}