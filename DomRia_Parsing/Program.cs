using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Text;
using Newtonsoft.Json;
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
        static string url_parse ="https://dom.ria.com/node/searchEngine/v2/?links-under-filter=on&category=1&realty_type=2&operation_type=1&fullCategoryOperation=1_2_1&wo_dupl=1&page=0&state_id=12&city_id=12&limit=20&sort=inspected_sort&period=per_hour&ch=242_239,247_252,265_0,1437_1436:";
        //static string url_parse = "https://dom.ria.com/node/searchEngine/v2/?links-under-filter=on&category=1&realty_type=2&operation_type=1&fullCategoryOperation=1_2_1&wo_dupl=1&page=0&state_id=12&city_id=12&limit=20&sort=inspected_sort&period=per_day&ch=242_239,247_252,265_0,1437_1436:";
        static IHtmlParser Parser = new HtmlParser();
        static List<string> AdsIdList = new List<string>() { "0"};
        public static async Task Main(string[] args)
        {
                for (int i = 0; ; i++)
                {
                    try
                    {
                        using (var client = new HttpClient())
                        {
                            IDocument html = await Parser.ParseDocumentAsync(await client.GetStringAsync(url_parse));
                            JObject json = JObject.Parse(html.Source.Text); //Если Item = 0, то исключение
                            if (json["count"].ToString().Contains("0")) {
                                await Task.Delay(2000);
                                continue;
                            }
                            JArray jsonArray = (JArray)json["items"];
                            List<string> list = jsonArray.Select(x => (string)x).ToList();
                            IEnumerable<string> listNewID = list.Except(AdsIdList);
                            foreach (string id in listNewID) {
                                AdsIdList.Add(id);
                                string urlAds = $"https://dom.ria.com/node/searchEngine/v2/view/realty/{id}?lang_id=4";
                                IDocument parseforAds = await Parser.ParseDocumentAsync(await client.GetStringAsync(urlAds));
                                JObject jsonAds = JObject.Parse(parseforAds.Source.Text);

                                string location = $"ул. {jsonAds["street_name"].ToString()}, р‑н. {jsonAds["district_name"].ToString()}, г. {jsonAds["city_name"].ToString()}";
                                string price = jsonAds["price"].ToString() + jsonAds["currency_type"].ToString();
                                string publishing_date = jsonAds["publishing_date"].ToString();
                                string beautiful_url = "https://dom.ria.com/ru/" + jsonAds["beautiful_url"].ToString();
                                string apartment = $"Комнат: {jsonAds["rooms_count"].ToString()} • {jsonAds["floor"].ToString()} этаж из {jsonAds["floors_count"].ToString()}  • {jsonAds["total_square_meters"].ToString()}м²";

                                var bot = new TelegramBotClient("1997294527:AAEkeioj8U3u7EaXkFftyxqcqnraVrcvVxs");
                                ChatId chatId = "-1001588392048";
                                bot.SendTextMessageAsync(chatId,
                                    $"<b><a href='{beautiful_url}' color='red'>{location}</a></b>\n" +
                                    $"<b>Стоимость:</b> {price}\n" +
                                    $"<b>Детально:</b> {apartment}\n" +
                                    $"<b>Время:</b> {publishing_date}\n", 
                                    ParseMode.Html).Wait();
                            }
                            await Task.Delay(2000);
                        }
                    }
                    catch (Exception e) {
                        Console.WriteLine(e.Message);
                    }
                }
        }
    }
}
