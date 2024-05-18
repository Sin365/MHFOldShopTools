using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MHFOldShopTools
{
    internal class Program
    {
        static string loc = Path.GetDirectoryName(AppContext.BaseDirectory) + "\\";

        const string InDir = "Files";
        const string Ver = "0.1";

        static void Main(string[] args)
        {
            string title = $"MHFOldShopTools Ver.{Ver} By 皓月云 axibug.com";
            Console.Title = title;
            Console.WriteLine(title);


            if (!Directory.Exists(loc + InDir))
            {
                Console.WriteLine("Files文件不存在");
                Console.ReadLine();
                return;
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);


            int bflag = 0;

            string[] files = FileHelper.GetDirFile(loc + InDir);
            while (true)
            {
                Console.WriteLine($"请确保Files目录中已放置解密的MHF-FW5 mhfdat.bin文件，请选择:");
                Console.WriteLine($"[1]，解析Files目录中bin文件，生成同名txt清单和csv表");
                Console.WriteLine($"[2]，解析Files目录中csv文件，修改回同名的bin文件中");
                Console.WriteLine($"[3]，探索道具数据");
                Console.WriteLine($"Please ensure that the decrypted MHF-FW5 mhfdat.bin file is placed in the Files directory. Please select:");
                Console.WriteLine($"[1],Parse the .bin file in the Files directory and generate a .txt list and .csv table with the same name");
                Console.WriteLine($"[2],Parse the .csv file in the Files directory and modify it back to the .bin file with the same name");

                string yn = Console.ReadLine();
                if (yn.ToLower() == "1")
                    bflag = 1;
                else if(yn.ToLower() == "2")
                    bflag = 2;
                else if (yn.ToLower() == "3")
                    bflag = 3;

                if (bflag != 0)
                    break;
            }

            if (bflag == 1)
            {
                int index = 0;
                int errcount = 0;
                for (int i = 0; i < files.Length; i++)
                {
                    string FileName = files[i].Substring(files[i].LastIndexOf("\\"));

                    if (System.IO.Path.GetExtension(FileName).ToLower() != ".bin")
                    {
                        continue;
                    }
                    index++;

                    Console.WriteLine($">>>>>>>>>>>>>>开始处理 第{index}个文件  {FileName}<<<<<<<<<<<<<<<<<<<");
                    FileHelper.LoadFile(files[i], out byte[] data);

                    ReaderItems(data, out List<string> OutInputString, out List<string> outPutCsv);


                    string listfileName = System.IO.Path.GetFileNameWithoutExtension(FileName) + ".txt";
                    string listoutpath = loc + InDir + "\\" + listfileName;
                    FileHelper.SaveFile(listoutpath, OutInputString.ToArray());

                    string csvfileName = System.IO.Path.GetFileNameWithoutExtension(FileName) + ".csv";
                    string csvoutpath = loc + InDir + "\\" + csvfileName;
                    FileHelper.SaveFile(csvoutpath, outPutCsv.ToArray());

                    Console.WriteLine($">>>>>>>>>>>>>>处理完毕>>>>>>>>>>>>>>");
                }
            }
            else if (bflag == 2)
            {
                int index = 0;
                int errcount = 0;
                for (int i = 0; i < files.Length; i++)
                {
                    string FileName = files[i].Substring(files[i].LastIndexOf("\\"));

                    if (System.IO.Path.GetExtension(FileName).ToLower() != ".csv")
                    {
                        continue;
                    }
                    index++;

                    Console.WriteLine($">>>>>>>>>>>>>>开始处理 第{index}个文件  {FileName}<<<<<<<<<<<<<<<<<<<");
                    FileHelper.LoadFile(files[i], out string[] lines);
                    List<ShopItem> itemlist = LoadStructForCsv(lines);


                    string binfileName = System.IO.Path.GetFileNameWithoutExtension(FileName) + ".bin";
                    string binoutpath = loc + InDir + "\\" + binfileName;

                    FileHelper.LoadFile(binoutpath, out byte[] bindata);

                    ModifyItem(bindata, itemlist, out byte[] ResultData);

                    FileHelper.SaveFile(binoutpath, ResultData);

                    Console.WriteLine($">>>>>>>>>>>>>>处理完毕>>>>>>>>>>>>>>");
                }
            }
            else if (bflag == 3)
            {
                int index = 0;
                int errcount = 0;
                for (int i = 0; i < files.Length; i++)
                {
                    string FileName = files[i].Substring(files[i].LastIndexOf("\\"));

                    if (System.IO.Path.GetExtension(FileName).ToLower() != ".bin")
                    {
                        continue;
                    }
                    index++;

                    Console.WriteLine($">>>>>>>>>>>>>>开始处理 第{index}个文件  {FileName}<<<<<<<<<<<<<<<<<<<");
                    FileHelper.LoadFile(files[i], out byte[] data);

                    DiscoverItems(data, out List<string> OutInput, out List<string> outPutCsv);


                    //string listfileName = System.IO.Path.GetFileNameWithoutExtension(FileName) + ".txt";
                    //string listoutpath = loc + InDir + "\\" + listfileName;
                    //FileHelper.SaveFile(listoutpath, OutInputString.ToArray());

                    //string csvfileName = System.IO.Path.GetFileNameWithoutExtension(FileName) + ".csv";
                    //string csvoutpath = loc + InDir + "\\" + csvfileName;
                    //FileHelper.SaveFile(csvoutpath, outPutCsv.ToArray());

                    Console.WriteLine($">>>>>>>>>>>>>>处理完毕>>>>>>>>>>>>>>");
                }

            }

            while (true)
            { 
                Console.ReadLine();
            }
        }


        static int NPStore_Ptr = 0x537924;
        const int _singelItemDatalenght = 12;
        static int NPStore_ItemCount = 712;

        static void ReaderItems(byte[] data,out List<string> OutInput,out  List<string> outPutCsv)
        {
            OutInput = new List<string>();
            outPutCsv = new List<string>();
            List<ShopItem> items = new List<ShopItem>();
            for (int i = 0; i < NPStore_ItemCount; i++)
            {
                items.Add(GetShopItemInfo(data, NPStore_Ptr + (i * _singelItemDatalenght)));
            }

            for (int i = 0; i < items.Count; i++)
            {
                ShopItem item = items[i];
                string ItemInfo;
                if (item.UnKnow)
                    ItemInfo = $"{"0x" + item.Ptr.ToString("X") + ":"} |   解析失败";
                else
                    ItemInfo = $"{"0x" + item.Ptr.ToString("X") + ":"} |   {item.ItemID}   ({MHHelper.Get2MHFItemName(item.ItemID)})   |   {item.Point}点    |   {item.Group}（{GetShopName(item.Group)}） {item.LevelType}({GetLevelTypeName(item.LevelType)})| [{item.OtherData[0].ToString("X")} {item.OtherData[1].ToString("X")} {item.OtherData[2].ToString("X")} {item.OtherData[3].ToString("X")}]";

                OutInput.Add(ItemInfo);
                outPutCsv.Add($"{item.ItemID},{item.Point},{item.Group},{item.LevelType},{MHHelper.Get2MHFItemName(item.ItemID)}");
                Console.WriteLine(ItemInfo);
            }

            /*
            int TempPtr = 0x539A78;
            int ToUpCount = 712;
            for (int i = 0; i < ToUpCount; i++)
            {
                items.Add(GetShopItemInfo(data, TempPtr + (-1 * i * _singelItemDatalenght)));
            }

            for (int i = items.Count - 1; i >= 0; i--)
            {
                ShopItem item = items[i];
                string ItemInfo;
                if (item.UnKnow)
                    ItemInfo = $"{"0x" + item.Ptr.ToString("X") + ":"} |   解析失败";
                else
                    ItemInfo = $"{ "0x" + item.Ptr.ToString("X") + ":"} |   {item.ItemID}   ({MHHelper.Get2MHFItemName(item.ItemID)})   |   {item.Point}点    |   {item.Group}（{GetShopName(item.Group)}） {item.LevelType}({GetLevelTypeName(item.LevelType)})| [{item.OtherData[0].ToString("X")} {item.OtherData[1].ToString("X")} {item.OtherData[2].ToString("X")} {item.OtherData[3].ToString("X")}]";

                Console.WriteLine(ItemInfo);
            }
            */
        }

        static void DiscoverItems(byte[] data, out List<string> OutInput, out List<string> outPutCsv)
        {
            OutInput = new List<string>();
            outPutCsv = new List<string>();
            List<ShopItem> items = new List<ShopItem>();
            

            int TempPtr = 0x537924;
            int ToUpCount = 712;

            int flag = 2;
            //顺序
            if (flag == 1)
            {
                for (int i = 0; i < ToUpCount; i++)
                {
                    items.Add(GetShopItemInfo(data, TempPtr + (1 * i * _singelItemDatalenght)));
                }

                for (int i = 0; i < items.Count; i++)
                {
                    ShopItem item = items[i];
                    string ItemInfo;
                    if (item.UnKnow)
                        ItemInfo = $"{"0x" + item.Ptr.ToString("X") + ":"} |   解析失败";
                    else
                        ItemInfo = $"{"0x" + item.Ptr.ToString("X") + ":"} |   {item.ItemID}   ({MHHelper.Get2MHFItemName(item.ItemID)})   |   {item.Point}点    |   {item.Group}（{GetShopName(item.Group)}） {item.LevelType}({GetLevelTypeName(item.LevelType)})| [{item.OtherData[0].ToString("X")} {item.OtherData[1].ToString("X")} {item.OtherData[2].ToString("X")} {item.OtherData[3].ToString("X")}]";

                    Console.WriteLine(ItemInfo);
                }
            }
            else //倒叙
            {
                {
                    for (int i = ToUpCount - 1; i >= 0; i--)
                    {
                        items.Add(GetShopItemInfo(data, TempPtr + (-1 * i * _singelItemDatalenght)));
                    }

                    for (int i = 0; i < items.Count; i++)
                    {
                        ShopItem item = items[i];
                        string ItemInfo;
                        if (item.UnKnow)
                            ItemInfo = $"{"0x" + item.Ptr.ToString("X") + ":"} |   解析失败";
                        else
                            ItemInfo = $"{"0x" + item.Ptr.ToString("X") + ":"} |   {item.ItemID}   ({MHHelper.Get2MHFItemName(item.ItemID)})   |   {item.Point}点    |   {item.Group}（{GetShopName(item.Group)}） {item.LevelType}({GetLevelTypeName(item.LevelType)})| [{item.OtherData[0].ToString("X")} {item.OtherData[1].ToString("X")} {item.OtherData[2].ToString("X")} {item.OtherData[3].ToString("X")}]";

                        Console.WriteLine(ItemInfo);
                    }
                }
            }

        }

        static List<ShopItem> LoadStructForCsv(string[] lines)
        {
            List<ShopItem> itemList = new List<ShopItem>();
            for (int i = 0;i < lines.Length; i++)
            {
                string[] temp = lines[i].Split(',');
                ShopItem item = new ShopItem()
                {
                    ItemID = Convert.ToInt32(temp[0]),
                    Point = Convert.ToInt32(temp[1]),
                    Group = Convert.ToInt32(temp[2]),
                    LevelType = Convert.ToInt32(temp[3]),
                };
                itemList.Add(item);
            }
            return itemList;
        }

        static void ModifyItem(byte[] srcdata, List<ShopItem> items,out byte[] ResultData)
        {
            byte[] target = HexHelper.CopyByteArr(srcdata);

            //ClearData
            for (int i = NPStore_Ptr; i < NPStore_Ptr + (NPStore_ItemCount * _singelItemDatalenght); i++)
                target[i] = 0x00;

            for (int i = 0; i < items.Count; i++)
            {
                ShopItem itemdata = items[i];
                int tempItemIDPtr = NPStore_Ptr + (i * _singelItemDatalenght);
                int tempPricePtr = tempItemIDPtr + 4;
                int tempMenuPtr = tempItemIDPtr + 4 + 4;
                int tempLevelPtr = tempItemIDPtr + 4 + 4 + 1;
                HexHelper.ModifyDataToBytes(target, HexHelper.intToBytes(itemdata.ItemID), tempItemIDPtr);
                HexHelper.ModifyDataToBytes(target, HexHelper.intToBytes(itemdata.Point), tempPricePtr);
                target[tempMenuPtr] = (byte)itemdata.Group;
                target[tempLevelPtr] = (byte)itemdata.LevelType;
            }

            ResultData = target;
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
        
        static string GetLevelTypeName(int Level)
        {
            switch (Level)
            {
                case 0:return "无限制";
                case 1:return "HR31以上";
                case 2:return "HR100以上";
                case 3:return "SR1以上";
                case 4:return "SR31以上";
                default:
                    return "未定义";
            }
        }

        static ShopItem GetShopItemInfo(byte[] data, int StartPos)
        {
            int ItemID = -1;
            int Point = -1;
            int Group = -1;
            int LevelType = -1;
            bool UnKnow = false;
            try
            {
                ItemID = HexHelper.bytesToInt(data, 4, StartPos);
                Point = HexHelper.bytesToInt(data, 4, StartPos + 4);
                Group = HexHelper.bytesToInt(data, 1, StartPos + 4 + 4);
                LevelType = HexHelper.bytesToInt(data, 1, StartPos + 4 + 4 + 1);
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
                LevelType = LevelType,
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
            public int LevelType;
            public int[] OtherData;
            public bool UnKnow;
        }

    }
}
