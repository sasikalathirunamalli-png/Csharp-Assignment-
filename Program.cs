using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace CSharp_Assignment
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = "https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code=vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";
            HttpClient client = new HttpClient();
            string json = client.GetStringAsync(url).Result;
            var entries = JsonConvert.DeserializeObject<List<TimeEntry>>(json);

            var sorted = entries
                .Where(e => e.DeletedOn == null && !string.IsNullOrEmpty(e.EmployeeName))
                .GroupBy(e => e.EmployeeName)
                .Select(g => new { Name = g.Key, TotalHours = g.Sum(e => (e.EndTimeUtc - e.StarTimeUtc).TotalHours) })
                .OrderByDescending(x => x.TotalHours)
                .ToList();

            foreach (var emp in sorted)
                Console.WriteLine(emp.Name + " - " + emp.TotalHours.ToString("F2") + " hours");

            // PART A : HTML TABLE
            StringBuilder html = new StringBuilder();
            html.Append("<html><head><style>table{border-collapse:collapse;width:60%} th,td{border:1px solid black;padding:8px} th{background:#333;color:white} .less{background:#ffcccc}</style></head><body><h2>Employee Total Time Worked</h2><table><tr><th>Name</th><th>Total Time Worked (Hours)</th></tr>");
            foreach (var emp in sorted)
            {
                string cls = emp.TotalHours < 100 ? "class='less'" : "";
                html.Append("<tr " + cls + "><td>" + emp.Name + "</td><td>" + emp.TotalHours.ToString("F2") + "</td></tr>");
            }
            html.Append("</table></body></html>");
            System.IO.File.WriteAllText("employees.html", html.ToString());
            Console.WriteLine("HTML Created!");

            // PART B : PIE CHART PNG - System.Drawing
            double grandTotal = sorted.Sum(x => x.TotalHours);
            Bitmap bmp = new Bitmap(900, 600);
            Graphics gfx = Graphics.FromImage(bmp);
            gfx.Clear(Color.White);
            Brush[] brushes = { Brushes.Red, Brushes.Blue, Brushes.Green, Brushes.Orange, Brushes.Purple, Brushes.Brown, Brushes.Pink, Brushes.Teal, Brushes.Gold, Brushes.Gray };
            float startAngle = 0;
            for (int i = 0; i < sorted.Count; i++)
            {
                float sweep = (float)(sorted[i].TotalHours / grandTotal * 360);
                gfx.FillPie(brushes[i % brushes.Length], 50, 50, 400, 400, startAngle, sweep);
                startAngle += sweep;
                gfx.FillRectangle(brushes[i % brushes.Length], 500, 50 + i * 40, 20, 20);
                gfx.DrawString(sorted[i].Name + " - " + (sorted[i].TotalHours / grandTotal * 100).ToString("F1") + "%", new Font("Arial", 12), Brushes.Black, 530, 50 + i * 40);
            }
            bmp.Save("piechart.png", ImageFormat.Png);
            Console.WriteLine("Pie Chart Created!");
            Console.ReadLine();
        }
    }
}