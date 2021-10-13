using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DomRia_Parsing
{
    class Program
    {
        static string urlForParse = "https://dom.ria.com/node/searchEngine/v2/?links-under-filter=on&category=1&realty_type=2&operation_type=1&fullCategoryOperation=1_2_1&wo_dupl=1&page=0&state_id=12&city_id=12&limit=20&sort=inspected_sort&period=per_hour&ch=242_239,247_252,265_0,1437_1436:";
        //static string urlForParse = "https://dom.ria.com/node/searchEngine/v2/?links-under-filter=on&category=1&realty_type=2&operation_type=1&fullCategoryOperation=1_2_1&wo_dupl=1&page=0&state_id=12&city_id=12&limit=20&sort=inspected_sort&period=per_day&ch=242_239,247_252,265_0,1437_1436:";
        static readonly IHtmlParser parser = new HtmlParser();
        static readonly List<string> baceAdsIdList = new List<string>() { "0" };
        static readonly TelegramBotClient telegramTockenBotClient = new TelegramBotClient("1997294527:AAEkeioj8U3u7EaXkFftyxqcqnraVrcvVxs");
        static readonly ChatId telegramChatId = "-1001588392048";
        public static async Task Main(string[] args)
        {
            Console.WriteLine($"Start parsing {DateTime.Now}");
            for (int i = 0; ; i++)
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        IDocument html = await parser.ParseDocumentAsync(await client.GetStringAsync(urlForParse));
                        JObject json = JObject.Parse(html.Source.Text);
                        if (json["count"].ToString().Contains("0"))
                        {
                            await Task.Delay(2000); 
                            continue;
                        }
                        JArray jsonArray = (JArray)json["items"];
                        List<string> listId = jsonArray.Select(x => (string)x).ToList();
                        IEnumerable<string> listIdNewAds = listId.Except(baceAdsIdList);
                        foreach (string item in listIdNewAds)
                        {
                            Console.WriteLine($"New Ads {DateTime.Now}");
                            string urlAds = $"https://dom.ria.com/node/searchEngine/v2/view/realty/{item}?lang_id=4";
                            IDocument parseforAds = await parser.ParseDocumentAsync(await client.GetStringAsync(urlAds));
                            JObject jsonAds = JObject.Parse(parseforAds.Source.Text);

                            string location = $"ул. {jsonAds["street_name"]}, р‑н. {jsonAds["district_name"]}, г. {jsonAds["city_name"]}";
                            string price = $"{jsonAds["price"]}{jsonAds["currency_type"]}";
                            string publishing_date = $"{jsonAds["publishing_date"]}";
                            string beautiful_url = "https://dom.ria.com/ru/" + $"{jsonAds["beautiful_url"]}";
                            string apartment = $"Комнат: {jsonAds["rooms_count"]} • {jsonAds["floor"]} этаж из {jsonAds["floors_count"]} • {jsonAds["total_square_meters"]}м²";
                            string telegramMessage =
                                $"<b><a href='{beautiful_url}'>{location}</a></b>\n" +
                                $"<b>Стоимость:</b> {price}\n" +
                                $"<b>Детально:</b> {apartment}\n" +
                                $"<b>Дата публикции:</b> {publishing_date}\n";

                            telegramTockenBotClient.SendTextMessageAsync(telegramChatId, telegramMessage, ParseMode.Html).Wait();
                            baceAdsIdList.Add(item);
                        }
                        await Task.Delay(2000);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
