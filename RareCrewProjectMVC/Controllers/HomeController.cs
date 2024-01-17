using ChartJSCore.Models;
using Microsoft.AspNetCore.Mvc;
using RareCrewProjectMVC.Core;
using RareCrewProjectMVC.Models;
using System.Diagnostics;
using System.Web;

namespace RareCrewProjectMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppSettings _appSettings;
        //private string ApiUrl = "https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code=vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";
        public HomeController(ILogger<HomeController> logger,AppSettings settings)
        {
            _logger = logger;
            _appSettings = settings;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                List<EmployeeModel> employees = await GetEmployees();
                
                if (employees != null && employees.Count > 0) 
                {

                    var groupedEmployees = employees.GroupBy(e => e.EmployeeName)
                        .Select(g => new EmployeeModel
                        {
                            EmployeeName = g.Key,
                            TotalTimeWorked = g.Sum(e=>e.HoursWorked)
                        })
                        .OrderByDescending(e => e.TotalTimeWorked)
                        .ToList();
                        ;
                    CreateGraph(groupedEmployees);

                    return View(groupedEmployees);
                }

                else
                {
                    ViewBag.Message = "No data";
                    return View();
                }

            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error: {ex.Message}";
                return View();
            }

        }


        private async Task<List<EmployeeModel>> GetEmployees()
        {
            using(HttpClient client = new HttpClient())
            {
                HttpResponseMessage responseMessage = await client.GetAsync(_appSettings.ApiUrl);

                if (responseMessage.IsSuccessStatusCode)
                {
                    
                    return await responseMessage.Content.ReadFromJsonAsync<List<EmployeeModel>>();
                }
                else
                {
                    throw new Exception($"Failed to retrieve data from the API. Status code: {responseMessage.StatusCode}");
                }
            }
        }



        public IActionResult Privacy()
        {
            return View();
        }

        public void CreateGraph(List<EmployeeModel> employees)
        {

            var chartData = new List<ChartData>();
        
            chartData = employees.Select(x=> new ChartData
            {
               
                Label = x.EmployeeName,
                Value = x.TotalTimeWorked
            }).ToList();
            ViewBag.ChartData = chartData;

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
