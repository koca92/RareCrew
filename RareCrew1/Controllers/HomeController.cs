using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RareCrew1.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using static System.Net.WebRequestMethods;

namespace RareCrew1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private async Task<string> GetJson()
        {
            try
            {
                HttpClient client = new HttpClient();
                var code = "vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";
                string url = $"https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code={code}";
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string json = "";
                json = response.Content.ReadAsStringAsync().Result;
                return json;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {

            var json = await GetJson();
            Debug.WriteLine(json);
            List<JsonObj> jsonList = new List<JsonObj>();
            if (json != null)
            {
                jsonList = JsonConvert.DeserializeObject<List<JsonObj>>(json);
            }
            else throw new Exception("Empty json");
            List<Employee> employees = new List<Employee>();
            foreach (var j in jsonList.DistinctBy(x => x.Id).ToList())
            {
                var employee = new Employee { Name = j.EmployeeName, Hours = CalculateHours(j.StarTimeUtc, j.EndTimeUtc) };
                if (employees.Exists(p => p.Name == employee.Name))
                {
                    employees.FirstOrDefault(p => p.Name == employee.Name).Hours += employee.Hours;
                }
                else
                {
                    employees.Add(employee);
                }
            }
            return View("Index", employees);


        }

        public IActionResult Privacy()
        {
            return View();
        }

        private double CalculateHours(DateTime Start,DateTime End)
        {
            TimeSpan ts = End- Start;
            return ts.TotalHours;
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}