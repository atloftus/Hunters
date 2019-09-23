using System;
using System.Text;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;


namespace Hunters
{
    /// <summary>
    ///     This class allows reading and writing to a google sheet.
    /// </summary>
    public class GoogleDriveService
    {
        #region PROPERTIES
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string ApplicationName = "LIHunter";
        GoogleCredential Credential { get; set; }
        public string Sheet { get; set; } = "LinkedIn";
        public string Range { get; set; } = "LinkedIn!A2:I";
        public string SpreadsheetID { get; set; } = "1Q70wUYzkFZcPbrF0ttrzffIlrEzlBfYH58pKx4x0nbY";
        SheetsService Service { get; set; }
        public List<Job> Jobs { get; set; }
        #endregion


        #region CONSTRUCTORS
        /// <summary>
        ///     This is the default constructor for the GoogleDriveService class that creates the Google credential and SheetsService.
        /// </summary>
        public GoogleDriveService()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            var directorySplit = currentDirectory.Split(new string[] { "LIHunter" }, StringSplitOptions.None);
            var secretLocation = directorySplit[0] + "client_secrets.json";

            using (var stream = new FileStream(secretLocation, FileMode.Open, FileAccess.Read))
            {
                this.Credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
            }

            this.Service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = this.Credential,
                ApplicationName = ApplicationName,
            });
        }


        /// <summary>
        ///     This is an accessory constructor that entends the default constructor and tkaes in a list of Jobs and puts them
        ///     in the Job property of this class.
        /// </summary>
        /// <param name="jobs"></param>
        public GoogleDriveService(List<Job> jobs) : this()
        {
            Jobs = jobs;
        }
        #endregion


        #region METHODS
        /// <summary>
        ///     This method takes in a list of jobs and writes each one as a line item in the google sheet.
        /// </summary>
        /// <param name="jobs"></param>
        /// <returns></returns>
        public string CreateGoogleSheetsJobEntries(List<Job> jobs)
        {
            List<string> existingRfids = getExistingSheetJobRefIds();
            List<IList<object>> lineItems = new List<IList<object>>();
            List<object> lineHolder = new List<object>();
            foreach (Job job in jobs)
            {
                if (!(existingRfids.Contains(job.RefID)))
                {
                    lineHolder = new List<object>() { job.CompanyName, job.Location, job.Position, job.IsEasyApply, job.DatePosted, job.DateAddedToSheet, job.Details, job.Link, job.RefID };
                    lineItems.Add(lineHolder);
                }                 
            }

            var valueRange = new ValueRange();
            valueRange.Values = lineItems;
            var appendRequest = Service.Spreadsheets.Values.Append(valueRange, SpreadsheetID, Range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var updateReponse = appendRequest.Execute();
            return lineItems.Count.ToString();
        }


        /// <summary>
        ///     This method returns all of the refids that already exist in the home google sheet.
        /// </summary>
        /// <returns></returns>
        public List<string> getExistingSheetJobRefIds()
        {
            List<string> existingRfids = new List<string>();
            SpreadsheetsResource.ValuesResource.GetRequest request = Service.Spreadsheets.Values.Get(SpreadsheetID, Range);
            ValueRange response = request.Execute();
            IList<IList<Object>> values = response.Values;
            if (values != null) foreach (var row in values) existingRfids.Add(row[8].ToString());
            return existingRfids;
        }
        #endregion
    }
}