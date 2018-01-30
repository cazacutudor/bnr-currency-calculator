using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using CurrencyConverterApi.models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using SQLitePCL;

// Aplicatie de convertor valutar care isi ia automat cursul prin servicul web al BNR 

namespace CurrencyConverterApi.Controllers
{
    [Route("api/[controller]")]
    public class CurrencyController : Controller
    {
        private static HttpClient client = new HttpClient();

        // GET api/currency
        [HttpGet]
        public JsonResult Get()
        {
            return Json(GetCurrencyList());
        }

        // POST api/currency
        [HttpPost]
        public JsonResult Post([FromBody] ConvertRequest request)
        {
            List<Currency> currencyList = GetCurrencyList();
            ConvertResponse response = new ConvertResponse();


            Currency toCurrency = currencyList.Find(currency => currency.name == request.ToCurrency);
            if (toCurrency != null)
            {
                response.Value = request.Value;
                response.ToCurrency = request.ToCurrency;
                response.FromCurrency = "RON";
                response.Result = (request.Value / toCurrency.rate);
            }
            else
            {
                response.Error = "Currency can't be found";
            }

            return Json(response);
        }

        private List<Currency> GetCurrencyList()
        {
            XDocument content = fetchBNRCurrency().Result;

            XNamespace ns = "http://www.bnr.ro/xsd";

            XElement header = content.Root.Element(ns + "Header");
            string publisher = (string) header.Element(ns + "Publisher");
            string publishingDate = (string) header.Element(ns + "PublishingDate");

            IEnumerable<XElement> rates = content.Root.Element(ns + "Body").Element(ns + "Cube").Elements(ns + "Rate");

            List<Currency> currencyList = new List<Currency>();

            foreach (var rate in rates)
            {
                Currency currency = new Currency();
                currency.rate = float.Parse(rate.Value);
                currency.publisher = publisher;
                currency.publishDate = publishingDate;
                currency.name = (string) rate.Attribute("currency");

                currencyList.Add(currency);
            }

            return currencyList;
        }

        private async Task<XDocument> fetchBNRCurrency()
        {
            HttpResponseMessage response = await client.GetAsync("http://www.bnr.ro/nbrfxrates.xml");
            response.EnsureSuccessStatusCode();

            string rawContent = await response.Content.ReadAsStringAsync();
            return XDocument.Parse(rawContent);
        }
    }
}