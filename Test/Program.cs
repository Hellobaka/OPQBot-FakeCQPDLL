using Sdk.Cqp.Expand;
using System;
using System.IO;

namespace Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            long groupid = 114514;
            long qqid = 1919810;
            string name = "name名字";
            string card = "card名片";
            int sex = 1;
            int age = 10;
            string area = "sdaj";
            int ingroup = 1596086003;
            int lastsay = 1596697117;
            string touxian = "龙王";
            int manager = 0;
            int buliang = 0;
            string viptouxian = "rbq";
            int outdate = 1596897117;
            int allowedit = 1;

            MemoryStream stream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(stream);
            
            BinaryWriterExpand.Write_Ex(binaryWriter, 2);

            MemoryStream stream2 = new MemoryStream();
            BinaryWriter binaryWriter2 = new BinaryWriter(stream2);

            BinaryWriterExpand.Write_Ex(binaryWriter2, groupid);
            BinaryWriterExpand.Write_Ex(binaryWriter2, qqid);
            BinaryWriterExpand.Write_Ex(binaryWriter2, name);
            BinaryWriterExpand.Write_Ex(binaryWriter2, card);
            BinaryWriterExpand.Write_Ex(binaryWriter2, sex);
            BinaryWriterExpand.Write_Ex(binaryWriter2, age);
            BinaryWriterExpand.Write_Ex(binaryWriter2, area);
            BinaryWriterExpand.Write_Ex(binaryWriter2, ingroup);
            BinaryWriterExpand.Write_Ex(binaryWriter2, lastsay);
            BinaryWriterExpand.Write_Ex(binaryWriter2, touxian);
            BinaryWriterExpand.Write_Ex(binaryWriter2, manager);
            BinaryWriterExpand.Write_Ex(binaryWriter2, buliang);
            BinaryWriterExpand.Write_Ex(binaryWriter2, viptouxian);
            BinaryWriterExpand.Write_Ex(binaryWriter2, outdate);
            BinaryWriterExpand.Write_Ex(binaryWriter2, allowedit);

            BinaryWriterExpand.Write_Ex(binaryWriter, (short)stream2.Length);
            binaryWriter.Write(stream2.ToArray());

            stream2 = new MemoryStream();
            binaryWriter2 = new BinaryWriter(stream2);

            BinaryWriterExpand.Write_Ex(binaryWriter2, groupid);
            BinaryWriterExpand.Write_Ex(binaryWriter2, qqid);
            BinaryWriterExpand.Write_Ex(binaryWriter2, name);
            BinaryWriterExpand.Write_Ex(binaryWriter2, card);
            BinaryWriterExpand.Write_Ex(binaryWriter2, sex);
            BinaryWriterExpand.Write_Ex(binaryWriter2, age);
            BinaryWriterExpand.Write_Ex(binaryWriter2, area);
            BinaryWriterExpand.Write_Ex(binaryWriter2, ingroup);
            BinaryWriterExpand.Write_Ex(binaryWriter2, lastsay);
            BinaryWriterExpand.Write_Ex(binaryWriter2, touxian);
            BinaryWriterExpand.Write_Ex(binaryWriter2, manager);
            BinaryWriterExpand.Write_Ex(binaryWriter2, buliang);
            BinaryWriterExpand.Write_Ex(binaryWriter2, viptouxian);
            BinaryWriterExpand.Write_Ex(binaryWriter2, outdate);
            BinaryWriterExpand.Write_Ex(binaryWriter2, allowedit);

            BinaryWriterExpand.Write_Ex(binaryWriter, (short)stream2.Length);
            binaryWriter.Write(stream2.ToArray());

            File.WriteAllText("1.txt", Convert.ToBase64String(stream.ToArray()));
            Console.ReadKey();
        }
    }
}
