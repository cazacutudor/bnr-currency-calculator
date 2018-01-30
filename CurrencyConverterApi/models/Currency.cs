using System.Security.Cryptography.X509Certificates;

namespace CurrencyConverterApi.models
{
    public class Currency
    {
        public string name { get; set; }
        public float rate { get; set; }
        public string publisher { get; set; }
        public string publishDate { get; set; }
    }
}