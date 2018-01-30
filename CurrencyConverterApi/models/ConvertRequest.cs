using System;

namespace CurrencyConverterApi.models
{
    public class ConvertRequest
    {
        public int Value { get; set; }
        public String ToCurrency { get; set; }
    }
}