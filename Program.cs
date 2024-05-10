using System.Text;

namespace MHFOldShopTools
{
    internal class Program
    {
        static string loc = Path.GetDirectoryName(AppContext.BaseDirectory) + "\\";

        const string InDir = "Input";
        const string OutDir = "Out";
        const string Ver = "0.1";

        static void Main(string[] args)
        {
            string title = $"MHFOldShopTools Ver.{Ver} By 皓月云 axibug.com";
            Console.Title = title;
            Console.WriteLine(title);


            if (!Directory.Exists(loc + InDir))
            {
                Console.WriteLine("Input文件不存在");
                Console.ReadLine();
                return;
            }

            //if (!Directory.Exists(loc + OutDir))
            //{
            //    Console.WriteLine("Out文件不存在");
            //    Console.ReadLine();
            //    return;
            //}

            //if (!Directory.Exists(loc + PosFile2DosDir))
            //{
            //    Console.WriteLine("Templete文件不存在");
            //    Console.ReadLine();
            //    return;
            //}

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);


            string[] files = FileHelper.GetDirFile(loc + InDir);
            Console.WriteLine($"共{files.Length}个文件，是否处理? (y/n)");

            string yn = Console.ReadLine();
            if (yn.ToLower() != "y")
                return;

            int index = 0;
            int errcount = 0;
            for (int i = 0; i < files.Length; i++)
            {
                string FileName = files[i].Substring(files[i].LastIndexOf("\\"));

                if (!FileName.ToLower().Contains(".bin"))
                {
                    continue;
                }
                index++;

                Console.WriteLine($">>>>>>>>>>>>>>开始处理 第{index}个文件  {FileName}<<<<<<<<<<<<<<<<<<<");
                FileHelper.LoadFile(files[i], out byte[] data);

                ReaderItems(data);
                Console.WriteLine($">>>>>>>>>>>>>>处理完毕");
            }

            while (true)
            { 
                Console.ReadLine();
            }
        }


        static int StartPtr = 0x539A78;
        const int _singelItemDatalenght = 12;
        static void ReaderItems(byte[] data)
        {
            List<ShopItem> items = new List<ShopItem>();

            int ToUpCount = 712;
            for (int i = 0; i < ToUpCount; i++)
            {
                items.Add(GetShopItemInfo(data, StartPtr + (-1 * i * _singelItemDatalenght)));
            }

            for (int i = items.Count - 1; i >= 0; i--)
            {
                ShopItem item = items[i];
                string ItemInfo;
                if (item.UnKnow)
                    ItemInfo = $"{"0x" + item.Ptr.ToString("X") + ":"} |   解析失败";
                else
                    ItemInfo = $"{ "0x" + item.Ptr.ToString("X") + ":"} |   {item.ItemID}   ({MHHelper.Get2MHFItemName(item.ItemID)})   |   {item.Point}点    |   {item.Group}（{GetShopName(item.Group)}）| [{item.OtherData[0].ToString("X")} {item.OtherData[1].ToString("X")} {item.OtherData[2].ToString("X")} {item.OtherData[3].ToString("X")}]";

                Console.WriteLine(ItemInfo);
            }
        }

        static string GetShopName(int gourp)
        {
            switch (gourp)
            {
                case 0:return "基本道具";
                case 1:return "采集素材";
                case 2:return "素材（HR99以下）";
                case 3:return "汎用素材（HR100以上）";
                case 4:return "装饰品";
                case 5:return "其他道具";
                default:
                    return "未定义";
            }
        }

        static ShopItem GetShopItemInfo(byte[] data, int StartPos)
        {
            int ItemID = -1;
            int Point = -1;
            int Group = -1;
            bool UnKnow = false;
            try
            {
                ItemID = HexHelper.bytesToInt(data, 4, StartPos);
                Point = HexHelper.bytesToInt(data, 4, StartPos + 4);
                Group = HexHelper.bytesToInt(data, 1, StartPos + 4 + 4);
                UnKnow = false;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"获取错误"+ex.ToString());
                UnKnow = true;
            }
            ShopItem item = new ShopItem()
            {
                Ptr = StartPos,
                ItemID = ItemID,
                Point = Point,
                Group = Group,
                OtherData = new int[]{ data[StartPos + 4 + 4 + 0],data[StartPos + 4 + 4 + 1], data[StartPos + 4 + 4 + 2], data[StartPos + 4 + 4 + 3] },
                UnKnow = UnKnow
            };
            return item;
        }

        struct ShopItem
        {
            public int Ptr;
            public int ItemID;
            public int Point;
            public int Group;
            public int[] OtherData;
            public bool UnKnow;
        }

    }
}
