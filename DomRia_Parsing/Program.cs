using AngleSharp.Dom;
using AngleSharp.Html.Parser;
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
        static IHtmlParser Parser = new HtmlParser();
        static List<string> AdsIdList = new List<string>();
        public static async Task Main(string[] args)
        {
                for (int i = 0; ; i++)
                {
                    try
                    {
                        using (var client = new HttpClient())
                        {
                            List<IElement> adList = new List<IElement>();
                            for (int j = 1; j < 5; j++) {
                                string url_parse = "https://dom.ria.com/uk/prodazha-kvartir/kiev-2k/?page=" + j;
                                IDocument html = await Parser.ParseDocumentAsync(await client.GetStringAsync(url_parse));
                                List<IElement> tb = html.QuerySelectorAll(".ticket-clear").ToList();
                                foreach (var value in tb) {
                                    if (value.GetAttribute("id").ToString().Contains("newbuild"))
                                        continue;
                                    adList.Add(value);
                                }
                            }
                            if (AdsIdList.Count != 0)
                            {
                                Console.WriteLine("Итерация " + i + ". Поиск нового объявления");
                                SearchNewAd(adList);
                                await Task.Delay(2000);
                                continue;
                            }

                            Console.WriteLine("Получен стартовый список объявлений");
                            foreach (var index in adList)
                            {
                                if (index.GetAttribute("id").ToString().Contains("newbuild"))
                                    continue;
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
