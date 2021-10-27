using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Xml;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DomRia_Parsing
{
    class Program
    {
        //static string Url_parse = "https://dom.ria.com/node/searchEngine/v2/?links-under-filter=on&category=1&realty_type=2&operation_type=1&fullCategoryOperation=1_2_1&wo_dupl=1&page=0&state_id=12&city_id=12&limit=20&sort=inspected_sort&period=per_hour&ch=242_239,247_252,265_0,1437_1436:";
        static readonly string Url_parse = "https://dom.ria.com/node/searchEngine/v2/?links-under-filter=on&category=1&realty_type=2&operation_type=1&fullCategoryOperation=1_2_1&wo_dupl=1&page=0&state_id=12&city_id=12&limit=20&sort=inspected_sort&period=per_allday&ch=242_239,247_252,265_0,1437_1436:";
        static readonly IHtmlParser Parser = new HtmlParser();
        static readonly string Path = Directory.GetCurrentDirectory() + @"\listAds.txt";
        static readonly List<string> AdsIdList = GetAdsList();
        static readonly TelegramBotClient BotClient = new TelegramBotClient("1997294527:AAEkeioj8U3u7EaXkFftyxqcqnraVrcvVxs");
        static readonly ChatId ChatIdNumber = "-1001588392048";

        public static async Task Main(string[] args)
        {
            Console.WriteLine($"Start parsing {DateTime.Now}");
            for (int i = 0; ; i++)
            {
                Console.WriteLine($"\n> New Iteration #{i}. {DateTime.Now}");
                try
                {
                    using (var client = new HttpClient())
                    {
                        IDocument html = await Parser.ParseDocumentAsync(await client.GetStringAsync(Url_parse));
                        JObject json = JObject.Parse(html.Source.Text); //Если Item = 0, то исключение
                        if (json["count"].ToString().Equals("0"))
                        {
                            await Task.Delay(100000);
                            continue;
                        }
                        JArray jsonArray = (JArray)json["items"];
                        List<string> listIdFromSite = jsonArray.Select(x => (string)x).ToList();
                        IEnumerable<string> listIdNewAds = listIdFromSite.Except(AdsIdList);

                        Console.WriteLine("AdsIdList Cont - {0}\nlistIdFromSite Cont - {1}\nlistIdNewAds Cont - {2}", AdsIdList.Count(), listIdFromSite.Count(), listIdNewAds.Count());
                        foreach (string id in listIdNewAds)
                        {
                            Console.WriteLine($"New Ads {DateTime.Now}. Id = {id}");
                            string urlAds = $"https://dom.ria.com/node/searchEngine/v2/view/realty/{id}?lang_id=4";
                            IDocument parseforAds = await Parser.ParseDocumentAsync(await client.GetStringAsync(urlAds));
                            JObject jsonAds = JObject.Parse(parseforAds.Source.Text);
                            /*
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

                            bot.SendTextMessageAsync(chatId, telegramMessage, ParseMode.Html).Wait();
                            */
                            AdsIdList.Insert(0, id);
                            Console.WriteLine($"AdsIdList[0] = {AdsIdList[0]}");

                        }

                        WriteNewAdsInList(AdsIdList);
                        await Task.Delay(100000);
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                await Task.Delay(100000);
            }
        }

        static List<string> GetAdsList()
        {
            new FileStream(Path, FileMode.OpenOrCreate).Close();
            using (XmlReader streamReader = new XmlTextReader(Path))
            {
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(List<string>));
                try
                {
                    return jsonSerializer.ReadObject(streamReader) as List<string>;
                }
                catch
                {
                    return new List<string>();
                }
            }
        }

        async static void WriteNewAdsInList(List<string> value)
        {
            await using (XmlWriter streamWriter = new XmlTextWriter(Path, null))
                    new DataContractJsonSerializer(typeof(List<string>)).WriteObject(streamWriter, AdsIdList);
            Console.WriteLine($"Object Serialize. Count = {value.Count()}");
        }
    }
}
