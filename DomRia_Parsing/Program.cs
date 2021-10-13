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
        static string url_parse =
            "https://dom.ria.com/node/searchEngine/v2/?links-under-filter=on&category=1&realty_type=2&operation_type=1&fullCategoryOperation=1_2_1&wo_dupl=1&page=0&state_id=12&city_id=12&limit=20&sort=inspected_sort&period=per_hour&ch=242_239,247_252,265_0,1437_1436:";
        static IHtmlParser Parser = new HtmlParser();
        static List<string> AdsIdList = new List<string>() { "0"};
        public static async Task Main(string[] args)
        {
            try
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

                            JArray plans = (JArray)json["items"];
                            if (AdsIdList.Count == 0) {
                                AdsIdList.AddRange(plans.Select(x => (string)x).ToList());
                                Console.WriteLine($"Итерация {i}. Получен список объявлений. Колличество {AdsIdList.Count()}");
                                await Task.Delay(2000);
                                //continue;
                            }
                            List<string> list = plans.Select(x => (string)x).ToList();
                            IEnumerable<string> exceptIdList = list.Except(AdsIdList); ;
                            
                            foreach (string value in exceptIdList) {
                                Console.WriteLine($"Объявления найденно. Id = {value}") ;
                                AdsIdList.Add(value);
                                string urlParse = $"https://dom.ria.com/node/searchEngine/v2/view/realty/{value}?lang_id=4";
                                IDocument searcheForId = await Parser.ParseDocumentAsync(await client.GetStringAsync(urlParse));
                                JObject j = JObject.Parse(searcheForId.Source.Text);
                                Console.WriteLine(j.Root);
                                Console.WriteLine("===========================================================");
                            }
                            
                        }
                    }
                    catch (Exception e) {
                        Console.WriteLine(e.Message);
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }

        static void SearchNewAd(List<IElement> TableHtml)
        {
            foreach (var index in TableHtml)
            {
                string adsId = index.GetAttribute("id").ToString();
                if (AdsIdList.Contains(adsId))
                    continue;
                Console.WriteLine("Объявление найдено");
                AdsIdList.Add(adsId);
                Program.NewAd(index);
            }
        }

        static void NewAd(IElement value)
        {
            String title = value.QuerySelector("div.wrap_desc > h3 > a > span").InnerHtml;
            String url = "https://dom.ria.com/" + value.QuerySelector("div.wrap_desc > h3 > a").GetAttribute("href");
            String price = value.QuerySelector("div.wrap_desc > div.mb-5.mt-10.pr > b.green.size22").InnerHtml;
            String location = value.QuerySelector("div.wrap_desc > h3 > span > a:nth-child(1)").TextContent;
            String time = value.QuerySelector("div.wrap_desc > div.mt-10.clear-foot > div > time").TextContent;
            Console.WriteLine(title);
            string token = "1997294527:AAEkeioj8U3u7EaXkFftyxqcqnraVrcvVxs";
            var bot = new TelegramBotClient(token);
            ChatId chatId = "-1001588392048";
            bot.SendTextMessageAsync(chatId, $"<b>{title}</b>\n<b>Стоимость:</b> {price}\n<b>Локация:</b> {location}\n<b>Время:</b> {time}\n<a href='{url}'>Ссылка</a>", ParseMode.Html).Wait();
        }

    }
}
