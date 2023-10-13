using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HtmlAgilityPack;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.Web.WebView2.Core;

namespace HitchTrackerStalk
{
    public class DistanceItem
    {
        public string Distance { get; set; }
        public string ShowDistance => Distance + "km";
    }

    public partial class MainWindow : Window
    {

        ObservableCollection<DistanceItem> _distanceItems;

        public MainWindow()
        {
            _distanceItems = new ObservableCollection<DistanceItem>();
            InitializeComponent();

            distanceListView.SelectionChanged += ListView_SelectionChanged;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            reset();
            getDistance();
        }

        private void Button_Click_Reset(object sender, RoutedEventArgs e)
        {
            reset();
        }

        private async void getDistance()
        {
            string html = await WebView.ExecuteScriptAsync($"document.querySelectorAll('.tUEI8e ').length");

            if (int.TryParse(html, out int elementCount) && elementCount > 0)
            {
                var script = $"Array.from(document.querySelectorAll('.tUEI8e')).map(e => e.innerHTML).join('\\n')";
                string htmlContent = await WebView.ExecuteScriptAsync(script);

                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(htmlContent);

                MatchCollection matches = Regex.Matches(htmlContent, @"(\d+\.\d+)\s*km");

                foreach (Match match in matches)
                {
                    string distance = match.Groups[1].Value;

                    _distanceItems.Add(new DistanceItem {  Distance = distance });
                }

                distanceListView.ItemsSource = _distanceItems;
            }

            IEnumerable<string> tags = HtmlAgilityPackParse(html);
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                DistanceItem selectedItem = (DistanceItem)e.AddedItems[0];

                double price = CalculatePrice(selectedItem.Distance);

                priceLabel.Content = $"Price: ${price:0.00}";
            }
        }

        private double CalculatePrice(string km)
        {
            double startPrice = 5;

            double maxPrice = Convert.ToDouble(km) * 2.65 + startPrice;

            return maxPrice;
        }

        private void reset()
        {
            _distanceItems.Clear();
            priceLabel.Content = "Price: ";
        }

        private IEnumerable<string> HtmlAgilityPackParse(string html)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            List<string> hrefTags = new List<string>();

            var htmlBody = htmlDocument.DocumentNode.SelectSingleNode("//body");

            if (htmlBody != null)
            {
                var hrefElements = htmlBody.SelectNodes("//a");
                if (hrefElements != null)
                {
                    foreach (var hrefElement in hrefElements)
                    {
                        hrefTags.Add(hrefElement.OuterHtml);
                    }
                }
            }

            return hrefTags;
        }
    }
}
