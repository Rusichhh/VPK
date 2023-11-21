using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.CSharp.RuntimeBinder;

[Route("api/currency")]
[ApiController]
public class CurrencyController : ControllerBase
{
    private readonly HttpClient httpClient;
    private const string ExchangeRateApiUrl = "https://www.cbr-xml-daily.ru/daily_json.js"; // URL API Центробанка
    private decimal markupPercentage = 0.05m; // 5% наценки

    public CurrencyController()
    {
        httpClient = new HttpClient();
    }

    [HttpGet]
    public async Task<IActionResult> ConvertCurrencyWithMarkup(decimal amount, string fromCurrency)
    {


        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(ExchangeRateApiUrl);
                Console.WriteLine(response.Content);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    

                    if (!string.IsNullOrEmpty(json))
                    {
                        CBRCurrencyRates exchangeRates = JsonSerializer.Deserialize<CBRCurrencyRates>(json);

                        if (exchangeRates != null && exchangeRates.Valute != null &&
                            exchangeRates.Valute.TryGetValue(fromCurrency, out CBRCurrencyRate fromRate)) //&&
                            //exchangeRates.Valute.TryGetValue(toCurrency, out CBRCurrencyRate toRate))
                        {
                            decimal convertedAmount = (amount * fromRate.Value);

                            decimal markedUpAmount = convertedAmount * (1 + markupPercentage);
                            return Ok(markedUpAmount);
                        }
                        else
                        {
                            return BadRequest("Невозможно найти курсы обмена для указанных валют.");
                        }
                    }
                    else
                    {
                        return BadRequest("JSON-данные отсутствуют или некорректны.");
                    }
                }
                else
                {
                    return BadRequest("Ошибка при получении данных о курсах валют.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Ошибка при конвертации валюты с наценкой: " + ex.Message);
            }
        }
    }

    public class CBRCurrencyRates
    {
        public Dictionary<string, CBRCurrencyRate>? Valute { get; set; }
    }

    public class CBRCurrencyRate
    {
        public string? CharCode { get; set; }
        public decimal Value { get; set; }
    }

}