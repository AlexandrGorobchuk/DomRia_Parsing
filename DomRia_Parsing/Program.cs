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
                            Console.WriteLine("Итерация " + i + ". Получен список объявлений");
                            foreach (var index in TableHtml)
                            {
                                AdsIdList.Add(index.GetAttribute("id").ToString());
                            }
                        }
                    }
                    catch (Exception e) {
                        Console.WriteLine(e.Message);
                    }
                }
        }

        static void SearchNewAd(List<IElement> TableHtml)
        {
            foreach (var index in TableHtml)
            {
                string adsId = index.GetAttribute("id").ToString();
                if (!AdsIdList.Contains(adsId))
                {
                    AdsIdList.Add(adsId);
                    NewAd(index);
                }
            }
        }

        static void NewAd(IElement value)
        {
            try
            {
                String title = value.QuerySelector("div.wrap_desc > h3 > a > span").TextContent;
                String url = "https://dom.ria.com/" + value.QuerySelector("div.wrap_desc > h3 > a").GetAttribute("href");
                String price = value.QuerySelector("div.wrap_desc > div.mb-5.mt-10.pr > b.green.size22").InnerHtml;
                String location = value.QuerySelector("div.wrap_desc > h3 > span > a:nth-child(1)").TextContent;
                String time = value.QuerySelector("div.wrap_desc > div.mt-10.clear-foot > div > time").TextContent;
                var bot = new TelegramBotClient("1997294527:AAEkeioj8U3u7EaXkFftyxqcqnraVrcvVxs");
                ChatId chatId = "-1001588392048";
                bot.SendTextMessageAsync(chatId, $"<b>{title}</b>\n<b>Стоимость:</b> {price}\n<b>Локация:</b> {location}\n<b>Время:</b> {time}\n<a href='{url}'>Ссылка</a>", ParseMode.Html).Wait();
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }
    }
}
