using Microsoft.AspNetCore.Mvc;
using RareCrewProjectMVC.Models;
using System.Diagnostics;

namespace RareCrewProjectMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private string apiUrl = "https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code=vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
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

            return View();
        }


        private async Task<List<EmployeeModel>> GetEmployees()
        {
            using(HttpClient client = new HttpClient())
            {
                HttpResponseMessage responseMessage = await client.GetAsync(apiUrl);

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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
