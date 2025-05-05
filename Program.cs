// See https://aka.ms/new-console-template for more information
using CSC365_Project3.Models;
using CSC365_Project3;
using System.Diagnostics;
using CsvHelper;
using System.Collections.Generic;

Stopwatch sw = Stopwatch.StartNew();
FileOperations fileOps = new();
CollectionsOperations collOps = new();


// --------------------------------------------------------------------------------------------
//  Read all the VAERSVAX files and collect only COVID19 records.
//  Also fills the HashSet CovidVaersIds with each (COVID19) VAERS_ID from the files
//  The Hashset is used in subsequent operations to only read in records from the other files
//  that have VAERS_IDs from COVID19 vaccines
// --------------------------------------------------------------------------------------------
sw.Restart();
HashSet<int> CovidVaersIds = [];
List<VaxInfo> vaersVaxRecords = fileOps.ReadAllVaxInfoFiles("Data", CovidVaersIds, ["COVID19", "COVID19-2"]);
sw.Stop();
Console.WriteLine($"Vaccine Information Read: {vaersVaxRecords.Count}.  The elapsed time was: {sw.ElapsedMilliseconds}ms");
Console.WriteLine($"There are {CovidVaersIds.Count} unique VAERS_IDs collected from the various VAERSVAX files");

// --------------------------------------------------------------------------------------------
//  Read all the Patient Data files and collect only records where the VAERS_ID on the incoming
//  record is in the Hashset CovidVaersIds
// --------------------------------------------------------------------------------------------
sw.Restart();
List<PatientRec> patientDataRecs = fileOps.ReadAllPatientRecFiles("Data", CovidVaersIds);
sw.Stop();
Console.WriteLine($"Patient Records Read: {patientDataRecs.Count}.  The elapsed time was: {sw.ElapsedMilliseconds}ms");

// --------------------------------------------------------------------------------------------
//  Read all the Symptom Data files and collect only records where the VAERS_ID on the incoming
//  record is in the Hashset CovidVaersIds
// --------------------------------------------------------------------------------------------
sw.Restart();
List<Symptoms> symptomRecs = fileOps.ReadAllSymptomsFiles("Data", CovidVaersIds);
sw.Stop();
Console.WriteLine($"Symptoms Records Read: {symptomRecs.Count}.  The elapsed time was: {sw.ElapsedMilliseconds}ms");

// --------------------------------------------------------------------------------------------
//  Create VAERS_COVID_DataApril25 csv output file containing the combination of info
//  from all 3 sources (this is the deliverable for Task 1)
// --------------------------------------------------------------------------------------------
sw.Restart();
IEnumerable<VaersReport> joinedList = collOps.GetCombinedDataSet(vaersVaxRecords, patientDataRecs, symptomRecs);
fileOps.WriteCSVFile(joinedList, "VAERS_COVID_DataApril25.csv");
sw.Stop();
Console.WriteLine($"VAERS_COVID_DataApril25.csv was created in {sw.ElapsedMilliseconds}ms and contains {joinedList.Count()} records\n");

// --------------------------------------------------------------------------------------------
//  Convert the VAERSDataApril_25.csv dataset into a transaction dataset,
//  where each row contains the following information:
//  VAERS_ID,VAX_MANU, RECVDATE, AGE_YRS, SEX, DIED, DATEDIED,VAX_DATE,
//  no_of_symptoms(k), symptom_1, symptom_2, ..., symptom_k
// --------------------------------------------------------------------------------------------

Dictionary<string, string> cats = fileOps.ReadCategoriesFile("Data\\categorizedSymptoms.csv");


// This data set is grouped by VAERS_ID and combines symptom data from multiple records within a VAERS_ID
sw.Restart();
List<VaersGrouped> MLData = collOps.CreateMLDataSet(joinedList, cats);
Console.WriteLine($"MLDataSet was created in {sw.ElapsedMilliseconds}ms and contains {MLData.Count()} records");
sw.Stop();


sw.Restart();
List<VaersTransaction> transactionData = collOps.CreateTransactionData(MLData);
fileOps.WriteCSVFile(transactionData, "transactionData.csv");
Console.WriteLine($"transactionData.csv was created in {sw.ElapsedMilliseconds}ms and contains {transactionData.Count()} records");
sw.Stop();

double initialMinSupport = .01;
double minSupport = .05;
double minConfidence = .50;

for (int i = 0; i < 2; i++)
{
    bool femaleOnly = i == 0;

    sw.Restart();
    Dictionary<string, (double Support, double Confidence)> results = collOps.AnalyzeTransactionDataSet(transactionData, 
                                                                                                        minSupport, 
                                                                                                        minConfidence, 
                                                                                                        femaleOnly, 
                                                                                                        initialMinSupport);
    Console.WriteLine($"Frequent itemsets were created in {sw.ElapsedMilliseconds}ms\n\n");
    sw.Stop();
    Console.WriteLine("***************************************************************");
    Console.WriteLine("Association rules determined with...");
    Console.WriteLine($"Initial Minimum Support: {initialMinSupport}");
    Console.WriteLine($"Secondary Minimum Support: {minSupport}");
    Console.WriteLine($"Minimum Confidence: {minConfidence}");
    Console.WriteLine($"Considering Gender: {(femaleOnly ? "Female" : "Male")}"); 
    Console.WriteLine("***************************************************************\n\n");

    Console.WriteLine("Support\tConfidence  Rule");
    foreach (var entry in results)
    {
        string rule = entry.Key;
        double support = entry.Value.Support;
        double confidence = entry.Value.Confidence;
        Console.WriteLine($"{support:F2}\t{confidence:F2}\t{rule}");
    }

}



//fileOps.WriteCSVFile(MLData
//       .Select(x => new
//       {
//           x.VAERS_ID,
//           x.VAX_MANU,
//           x.RECVDATE,
//           x.AGE_YRS,
//           x.SEX,
//           x.DIED,
//           x.DATEDIED,
//           x.VAX_DATE,
//           x.NumOfSymptoms,
//           allSymptoms = string.Join("|", x.Symptoms)
//       }), "VAERSData_ML.csv");


