using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CSC365_Project3.Models;
using CsvHelper.Configuration;
using CsvHelper;

namespace CSC365_Project3
{
    internal class FileOperations
    {
        /// <summary>
        /// Reads a Patient Record file (VAERSData) and converts each line into a patient record, and returns a list of all the patient recs in that file
        /// Optionally include a HashSet of VAERS_IDs
        /// When included, only patient records from the file with a VAERS_ID in the HashSet will be included in the results.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="OnlyTheseVaersIds">When passed in, the method will only include Patient Rec if the VAERS_ID is in the HashSet</param>
        /// <returns></returns>
        public List<PatientRec> ReadPatientRecFile(string fileName, HashSet<int>? OnlyTheseVaersIds = null)
        {
            List<PatientRec> rtn = new();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true, 
                BadDataFound = null 
            };

            using (var reader = new StreamReader(fileName))
            using (var csv = new CsvReader(reader, config))
            {
                // Loop through rows
                while (csv.Read())
                {
                    // Parse VAERS_ID and check conditions
                    if (csv.TryGetField<int>(0, out int vaers_id) && (OnlyTheseVaersIds == null || OnlyTheseVaersIds.Contains(vaers_id)))
                    {
                        var rec = new PatientRec
                        {
                            VAERS_ID = vaers_id,
                            RECVDATE = csv.GetField<string>(1),
                            STATE = csv.GetField<string>(2),
                            AGE_YRS = csv.TryGetField<decimal>(3, out var age) ? age : 0, // Defaults to 0 if parsing fails
                            CAGE_YR = csv.GetField<string>(4),
                            CAGE_MO = csv.GetField<string>(5),
                            SEX = csv.GetField<string>(6),
                            RPT_DATE = csv.GetField<string>(7),
                            SYMPTOM_TEXT = csv.GetField<string>(8).Replace(",", " "),
                            DIED = csv.GetField<string>(9),
                            DATEDIED = csv.GetField<DateTime?>(10),
                            L_THREAT = csv.GetField<string>(11),
                            ER_VISIT = csv.GetField<string>(12),
                            HOSPITAL = csv.GetField<string>(13),
                            HOSPDAYS = csv.GetField<string>(14),
                            X_STAY = csv.GetField<string>(15),
                            DISABLE = csv.GetField<string>(16),
                            RECOVD = csv.GetField<string>(17),
                            VAX_DATE = csv.GetField<DateTime?>(18),
                            ONSET_DATE = csv.GetField<string>(19),
                            NUMDAYS = csv.GetField<string>(20),
                            LAB_DATA = csv.GetField<string>(21),
                            V_ADMINBY = csv.GetField<string>(22),
                            V_FUNDBY = csv.GetField<string>(23),
                            OTHER_MEDS = csv.GetField<string>(24),
                            CUR_ILL = csv.GetField<string>(25),
                            HISTORY = csv.GetField<string>(26),
                            PRIOR_VAX = csv.GetField<string>(27),
                            SPLTTYPE = csv.GetField<string>(28),
                            FORM_VERS = csv.GetField<string>(29),
                            TODAYS_DATE = csv.GetField<string>(30),
                            BIRTH_DEFECT = csv.GetField<string>(31),
                            OFC_VISIT = csv.GetField<string>(32),
                            ER_ED_VISIT = csv.GetField<string>(33),
                            ALLERGIES = csv.GetField<string>(34)
                        };

                        rtn.Add(rec);
                    }
                }
            }

            return rtn;
        }

        /// <summary>
        /// Reads a Symptoms Record file (VAERSSYMPTOMS) and converts each line into a Symptoms object, and returns a list of all the Symptoms objects in that file
        /// Optionally include a HashSet of VAERS_IDs
        /// When included, only symptoms from the file with a VAERS_ID in the HashSet will be included in the results.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="OnlyTheseVaersIds">When passed in, the method will only include Patient Rec if the VAERS_ID is in the HashSet</param>
        /// <returns></returns>
        public List<Symptoms> ReadSymptomsFile(string fileName, HashSet<int>? OnlyTheseVaersIds = null)
        {
            List<Symptoms> rtn = new();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true, // Assumes the first row is a header
                BadDataFound = null // Ignore any malformed lines gracefully
            };

            using (var reader = new StreamReader(fileName))
            using (var csv = new CsvReader(reader, config))
            {
                // Loop through rows
                while (csv.Read())
                {
                    // Parse VAERS_ID and check conditions
                    if (csv.TryGetField<int>(0, out int vaers_id) && (OnlyTheseVaersIds == null || OnlyTheseVaersIds.Contains(vaers_id)))
                    {
                        var symp = new Symptoms
                        {
                            VAERS_ID = vaers_id,
                            SYMPTOM1 = csv.GetField<string>(1),
                            SYMPTOMVERSION1 = csv.GetField<string>(2),
                            SYMPTOM2 = csv.GetField<string>(3),
                            SYMPTOMVERSION2 = csv.GetField<string>(4),
                            SYMPTOM3 = csv.GetField<string>(5),
                            SYMPTOMVERSION3 = csv.GetField<string>(6),
                            SYMPTOM4 = csv.GetField<string>(7),
                            SYMPTOMVERSION4 = csv.GetField<string>(8),
                            SYMPTOM5 = csv.GetField<string>(9),
                            SYMPTOMVERSION5 = csv.GetField<string>(10)
                        };

                        rtn.Add(symp);
                    }
                }
            }

            return rtn;
        }

        /// <summary>
        /// Reads a Vaccine file (*VAERSVAX) and converts each line into a VaxInfo object and returns a list of all the VaxInfo objects sourced from the file
        /// Optionally include a Hashset and a VAX_TYPE 
        /// When both are included
        /// 1. Only records with a VAX_TYPE matching VaxType are collected
        /// 2. If a record is collected, the VAERS_ID is added to the OnlyTheseVaersIds hash set 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="OnlyTheseVaersIds"></param>
        /// <returns></returns>
        public List<VaxInfo> ReadVaxInfoFile(string fileName, HashSet<int>? OnlyTheseVaersIds = null, string[]? VaxTypes = null)
        {
            List<VaxInfo> rtn = new();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true, // Assumes the first row is a header
                BadDataFound = null // Ignore malformed data gracefully
            };

            using (var reader = new StreamReader(fileName))
            using (var csv = new CsvReader(reader, config))
            {
                // Loop through rows
                while (csv.Read())
                {
                    // Parse VAERS_ID and check conditions
                    if (csv.TryGetField<int>(0, out int vaers_id))
                    {
                        var vax = new VaxInfo
                        {
                            VAERS_ID = vaers_id,
                            VAX_TYPE = csv.GetField<string>(1),
                            VAX_MANU = csv.GetField<string>(2),
                            VAX_LOT = csv.GetField<string>(3),
                            VAX_DOSE_SERIES = csv.GetField<string>(4),
                            VAX_ROUTE = csv.GetField<string>(5),
                            VAX_SITE = csv.GetField<string>(6),
                            VAX_NAME = csv.GetField<string>(7)
                        };

                        // Check VaxTypes condition
                        if (VaxTypes == null || VaxTypes.Contains(vax.VAX_TYPE))
                        {
                            rtn.Add(vax);

                            // Update OnlyTheseVaersIds HashSet (if provided)
                            OnlyTheseVaersIds?.Add(vax.VAERS_ID);
                        }
                    }
                }
            }

            return rtn;
        }


        /// <summary>
        /// Reads all the Patient Rec files in the directory provided
        /// Optionally collects only records from the source files if they have a VaersId that is in the Hash set of Vaers Ids provided 
        /// </summary>
        /// <param name="dirName"></param>
        /// <param name="onlyTheseVaersIds"></param>
        /// <returns></returns>
        public List<PatientRec> ReadAllPatientRecFiles(string dirName, HashSet<int>? onlyTheseVaersIds = null)
        {
            List<PatientRec> rtn = new();

            foreach (string fileName in Directory.GetFiles(dirName, "*VAERSData.csv"))
            {
                rtn.AddRange(ReadPatientRecFile(fileName));
            }

            return rtn;
        }

        /// <summary>
        /// Reads all the Symptom files in the directory provided
        /// Optionally collects only records from the source files if they have a VaersId that is in the Hash set of Vaers Ids provided 
        /// </summary>
        /// <param name="dirName"></param>
        /// <param name="onlyTheseVaersIds"></param>
        /// <returns></returns>
        public List<Symptoms> ReadAllSymptomsFiles(string dirName, HashSet<int>? onlyTheseVaersIds = null)
        {
            List<Symptoms> rtn = new();

            foreach (string fileName in Directory.GetFiles(dirName, "*VAERSSYMPTOMS.csv"))
            {
                rtn.AddRange(ReadSymptomsFile(fileName, onlyTheseVaersIds));
            }

            return rtn;
        }


        /// <summary>
        /// Optionally collects only records from the source files if they have a VAX_TYPE matching the string parameter collectOnlyTheseVaxTypes provided
        /// When vaersIdsCollected is provided (is not null), this method with fill the Hash set with all the VAERS_ID collected
        /// The resulting hashset can be used when reading the other files to only collect records with a VAERS_ID in the Hash set
        /// </summary>
        /// <param name="dirName"></param>
        /// <param name="vaersIdsCollected"></param>
        /// <param name="collectOnlyTheseTypes"></param>
        /// <returns></returns>
        public List<VaxInfo> ReadAllVaxInfoFiles(string dirName, HashSet<int>? vaersIdsCollected = null, string[]? collectOnlyTheseVaxTypes = null)
        {
            List<VaxInfo> rtn = new();

            foreach (string fileName in Directory.GetFiles(dirName, "*VAERSVAX.csv"))
            {
                rtn.AddRange(ReadVaxInfoFile(fileName, vaersIdsCollected, collectOnlyTheseVaxTypes));
            }

            return rtn;
        }

        /// <summary>
        /// Given an IEnumerable of Anonymous type, write the CSV content to the file path.
        /// Deletes the file if it already exists.
        /// </summary>
        /// <param name="joinedList"></param>
        /// <param name="outputPath"></param>
        public void WriteCSVFile(IEnumerable<dynamic> joinedList, string outputPath)
        {
            // Delete the file if it exists
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                Quote = '"',
                Escape = '"',
                Mode = CsvMode.NoEscape, // Prevents additional escaping
                BadDataFound = null,
            };

            using (var writer = new StreamWriter(outputPath))
            using (var csv = new CsvWriter(writer, config))
            {
                // Write the records to the CSV
                csv.WriteRecords(joinedList);
            }
        }

        internal Dictionary<string, string> ReadCategoriesFile(string filePath)
        {
            var categories = new Dictionary<string, string>();

            try
            {
                foreach (var line in File.ReadLines(filePath))
                {
                    // Skip empty lines
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    // Split the line by the comma character
                    var parts = line.Split(',');

                    if (parts.Length == 2)
                    {
                        var key = parts[1].Trim(); // Second column as the key
                        var value = parts[0].Trim(); // First column as the value

                        // Add to dictionary if key is not empty
                        if (!string.IsNullOrEmpty(key))
                        {
                            categories[key] = value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
            }
            return categories;
        }




        //public IEnumerable<SymptomTask2> ReadSymptomsTask2Sort(string fileName)
        //{
        //    bool firstLine = true;
        //    List<SymptomTask2> rtn = new();
        //    using (StreamReader sr = new(fileName))
        //    {
        //        string line;
        //        while ((line = sr.ReadLine()) != null)
        //        {
        //            if (!firstLine)
        //            {
        //                string[] data = line.Split(',');
        //                // Add the record when:
        //                // * the incoming line contains a VAERS_ID which parses to an int AND
        //                // (
        //                // * there was no hashset passed OR
        //                // * the hashset passed has values and the incoming VAERS_ID is one of those values
        //                // )
        //                if (int.TryParse(data[0], out int vaers_id))
        //                {
        //                    {
        //                        SymptomTask2 symp = new SymptomTask2();
        //                        symp.VAERS_ID = vaers_id;
        //                        if (decimal.TryParse(data[1], out decimal age))
        //                        {
        //                            symp.AGE_YRS = age;
        //                        }
        //                        else
        //                        {
        //                            symp.AGE_YRS = 0; //data[1] was unable to parse to an integer value
        //                        }
        //                        symp.SEX = data[2];
        //                        symp.VAX_NAME = data[3];
        //                        symp.RPT_DATE = data[4];
        //                        symp.SYMPTOM = data[5];
        //                        symp.DIED = data[6];
        //                        symp.DATEDIED = data[7];
        //                        symp.SYMPTOM_TEXT = data[8];   
        //                        rtn.Add(symp);
        //                    }
        //                }
        //            }
        //            firstLine = false;
        //        }
        //        return rtn;
        //    }
        //}

        //public void WriteTask3Report(IEnumerable<Task3Grouping> task3DataSet, string fileName)
        //{

        //    using (StreamWriter writer = new StreamWriter(fileName))
        //    {
        //        int currentBucketDeathCount = 0;
        //        string currentBucket = task3DataSet.First().Bucket;
        //        foreach (Task3Grouping item in task3DataSet)
        //        {
        //            if (item.Bucket != currentBucket)
        //            {
        //                //write out the total death count for this grouping 
        //                WriteOutGroupTotal(currentBucketDeathCount, writer, currentBucket);
        //                currentBucket = item.Bucket;
        //                currentBucketDeathCount = 0;
        //            }
        //            currentBucketDeathCount += item.DeathCount;
        //            //WriteOutSymptomLine(item, writer); //Uncomment to see detail of sort
        //        }
        //        WriteOutGroupTotal(currentBucketDeathCount, writer, currentBucket);
        //    } 
        //}

        //private void WriteOutSymptomLine(Task3Grouping item, StreamWriter writer)
        //{
        //    writer.WriteLine($"{item.Bucket}\t{item.SEX}\t{item.VAX_NAME}\t{item.SYMPTOM}");
        //}

        //private void WriteOutGroupTotal(int currentBucketDeathCount, StreamWriter writer, string ageBucket)
        //{
        //    writer.WriteLine($"Number of deaths for Age Group {ageBucket}: {currentBucketDeathCount:N0}");
        //    writer.WriteLine();
        //    writer.WriteLine();

        //}
    }

}
