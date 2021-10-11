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
        static string url_parse = "https://dom.ria.com/uk/prodazha-kvartir/kiev-rayon-borshchagovka/?page=1";
        static IHtmlParser Parser = new HtmlParser();
        static List<string> AdsIdList = new List<string>();
        public static async Task Main(string[] args)
        {
            try
            {
                for (int i = 1; ; i++)
                {
                    try
                    {
                        using (var client = new HttpClient())
                        {
                            IDocument html = await Parser.ParseDocumentAsync(await client.GetStringAsync(url_parse));
                            List<IElement> TableHtml = html.GetElementsByClassName("ticket-clear").ToList();
                            if (AdsIdList.Count != 0)
                            {
                                Console.WriteLine("Итерация " + i + ". Поиск нового объявления");
                                SearchNewAd(TableHtml);
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
