using System;
using System.Collections;
using Microsoft.VisualBasic.FileIO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExifLib;
using System.IO;


namespace HtLib
{
    class ReName
    {
        static string relationFile = @"d:\projects\read_exif\Book3.txt";
        static string source = @"d:\projects\read_exif\20131220永兴岛";
        static string destination = @"d:\projects\read_exif\des";
        static string planeNum = "001";
        static string delim = "-";
        static bool bTime = false;
        static FileInfo[] targetFiles;

        public static bool Running = true;
        public static event Action<int,int> onRenameCompleted;
        public static event Action onCompleted;

        static public int GetTaskCount()
        {
            return targetFiles.Length;
        }

        static public void Init(string num,string rel,string src,string des,bool btime)
        {
            planeNum = num;
            relationFile = rel;
            source = src;
            destination = des;
            bTime = btime;

            targetFiles = GetFiles(source);
        }

        static public int cli(string[] args)
        {
            string planeNum = "";
            string relation = "";
            string source = "";
            string destination = "";
            bool btime = true;

            for (int i = 0; i < args.Length; i++) // Loop through array
            {
                string argument = args[i];
                if (argument == "-p" && ((i+1) < args.Length))
                {
                    planeNum = args[++i];
                }

                if (argument == "-r" && ((i+1) < args.Length))
                {
                    relation = args[++i];
                }

                if (argument == "-s" && ((i + 1) < args.Length))
                {
                    source = args[++i];
                }

                if (argument == "-d" && ((i + 1) < args.Length))
                {
                    destination = args[++i];
                }

                if (argument == "-t" && ((i + 1) < args.Length))
                {
                    switch (args[++i])
                    {
                        case "1":
                            btime = true;
                            break;
                        case "0":
                            btime = false;
                            break;
                    }
                }
            }

            if (planeNum == "" 
                || relation == ""
                || source == ""
                || destination == "")
            {

                Help();
                return 1;
            }

            Console.WriteLine("dafd");

            onRenameCompleted += DrawTextProgressBar;
            ReName.Init(planeNum, relation, source, destination, btime);
            ReName.Run();

            return 0;
        }

        static void Help()
        {
            Console.WriteLine("ReTool is a tiny tool that batchs renameing file name with specified relation file.");
            Console.WriteLine("");
            Console.WriteLine("Usage:");
            Console.WriteLine("ReTool -p [plane number]");
            Console.WriteLine("       -r [relation file path, this file format must be *TXT* or *CSV*. ]");
            Console.WriteLine("       -s [source image directory]");
            Console.WriteLine("       -d [destination image directory]");
            Console.WriteLine("       -t [add time, 1 add time or 0 not]");
            Console.WriteLine("");
            Console.WriteLine("Example:");
            Console.WriteLine(@"ReTool -p 001 -r c:\rel.txt -s c:\simg -d c:\dimg -t 1");
        }
           
        /// <summary>
        /// convert "a001.jpg" => "3800-20130101-130101-赤鼻岛.jpg"
        ///                      planenum-date-time-islandName.jpg
        /// dataTime is image's exif time
        /// islandName have a relation file, such as 'a001 => 赤鼻岛'
        /// planenum depends on customer's input. 
        /// </summary>
        static public void Run()
        {
            var relation = CreateRelation(relationFile);
            var tasks = targetFiles;

            int index = 0;
            int total = GetTaskCount();
            foreach(FileInfo task in tasks){
                if (!Running) break;

                RenameFile(task,relation);

                if (onRenameCompleted != null)
                {
                    onRenameCompleted(index,total);
                }
            }

            if (onCompleted != null)
            {
                onCompleted();
            }
        }

        private static void RenameFile(FileInfo task,Dictionary<string, string> relation)
        {
            List<string> newPathToken = new List<string>();

            // add plane number
            newPathToken.Add(planeNum);

            var dateTaken = GetExifTime(task.FullName);
            var fileName = SplitFileName(task.Name);
            var mapNames = ReplaceToken(fileName.Item1, relation);

            if (bTime)
            {
                // add date and time
                newPathToken.Add(dateTaken.ToString("yyyyMMdd"));
                newPathToken.Add(dateTaken.ToString("Hmmss"));
            }
            else
            {
                // add date
                newPathToken.Add(dateTaken.ToString("yyyyMMdd"));
            }

            // add map and suffix
            newPathToken.Add(mapNames + fileName.Item2);

            // create a path
            var path = Path.Combine(destination, string.Join(delim, newPathToken.ToArray()));

            // copy file
            Copy(task.FullName, path);
        }


        #region func 
        static void Copy(string path1, string path2)
        {
            File.Copy(path1, path2,true);
        }

        static Dictionary<string, string> CreateRelation(String name)
        {
            char delimiter = DetectCSVDelimiter(name);
            return CreateRelation(name, delimiter.ToString());
        }

        static string ReplaceToken(string prefix, Dictionary<string, string> map)
        {
            return ReplaceToken(prefix, '-', '-', map);
        }
        static DateTime GetExifTime(string name)
        {
            DateTime datePictureTaken;
            using (ExifReader reader = new ExifReader(name))
            {
                if (reader.GetTagValue<DateTime>(ExifTags.DateTimeDigitized,
                                                out datePictureTaken))
                {
                    Logger("Get " + name);
                }
                else
                {
                    Logger("No " + name );
                }
            }

            return datePictureTaken;
        }

        static FileInfo[] GetFiles(string path)
        {
            DirectoryInfo dires = new DirectoryInfo(path);
            FileInfo[] files = dires.GetFiles();
            return files;
        }

        static char DetectSplit(string name)
        {
            char bracket = '(';
            char minus = '-';
            char space = ' ';

            if (!(name.Split(space).Length == 1))
            {
                return space;
            }

            if (!(name.Split(bracket).Length == 1))
            {
                return bracket;
            }

            if (!(name.Split(minus).Length == 1))
            {
                return minus;
            } 

            return space;

        }

        static Tuple<string,string> SplitFileName(string name)
        {
            char separator = DetectSplit(name);
            return SplitFileName(name, separator);
        }

        static Tuple<string,string> SplitFileName(string name,char c)
        {
            string[] tokens = name.Split(c);
            string basename;
            string ext;

            if (tokens.Length == 1)
            {

                basename = System.IO.Path.GetFileNameWithoutExtension(tokens[0]);
                ext = System.IO.Path.GetExtension(tokens[0]);
            }
            else{
                basename = tokens[0];
                ext = c + tokens[1];

            }
            return new Tuple<string,string>(basename,ext);
        }

        static string ReplaceToken(string prefix, char c ,char join,Dictionary<string, string> map)
        {
            string[] tokens = prefix.Split(c);
            List<string> maps = new List<string>();

            foreach(string item in tokens)
            {
                if(map.ContainsKey(item))
                {
                    maps.Add(map[item]);
                }
                else{
                    maps.Add(item);
                }
            }
            return string.Join(join + "",maps.ToArray());
        }

        static Dictionary<string, string> CreateRelation(String name,String  delimiter)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            using (TextFieldParser parser = new TextFieldParser(name,System.Text.Encoding.Default))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters( delimiter);
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    result.Add(fields[0], fields[1]);
                }
            }
            return result;
        }

        static void Logger(String lines)
        {
            var name = DateTime.Now.ToString("yyyyMMdd") + ".log";
            System.IO.StreamWriter file = new System.IO.StreamWriter(name, true);
            file.WriteLine(lines);

            file.Close();

        }

        static char DetectCSVDelimiter(string name)
        {
            List<char> separators = new List<char> { ' ', '\t', ',', ';' };
            char result;
            using (TextReader reader = File.OpenText(name))
            {
                result = DetectDelimiter(reader, 3, separators);
            }
            return result;
        }

        static char DetectDelimiter(TextReader reader, int rowCount, IList<char> separators)
        {
            IList<int> separatorsCount = new int[separators.Count];

            int character;

            int row = 0;

            bool quoted = false;
            bool firstChar = true;

            while (row < rowCount)
            {
                character = reader.Read();

                switch (character)
                {
                    case '"':
                        if (quoted)
                        {
                            if (reader.Peek() != '"') // Value is quoted and 
                                // current character is " and next character is not ".
                                quoted = false;
                            else
                                reader.Read(); // Value is quoted and current and 
                            // next characters are "" - read (skip) peeked qoute.
                        }
                        else
                        {
                            if (firstChar) 	// Set value as quoted only if this quote is the 
                                // first char in the value.
                                quoted = true;
                        }
                        break;
                    case '\n':
                        if (!quoted)
                        {
                            ++row;
                            firstChar = true;
                            continue;
                        }
                        break;
                    case -1:
                        row = rowCount;
                        break;
                    default:
                        if (!quoted)
                        {
                            int index = separators.IndexOf((char)character);
                            if (index != -1)
                            {
                                ++separatorsCount[index];
                                firstChar = true;
                                continue;
                            }
                        }
                        break;
                }

                if (firstChar)
                    firstChar = false;
            }

            int maxCount = separatorsCount.Max();

            return maxCount == 0 ? '\0' : separators[separatorsCount.IndexOf(maxCount)];
        }

        #endregion
        private static void DrawTextProgressBar(int progress, int total)
        {
            //draw empty progress bar
            Console.CursorLeft = 0;
            Console.Write("["); //start
            Console.CursorLeft = 32;
            Console.Write("]"); //end
            Console.CursorLeft = 1;
            float onechunk = 30.0f / total;

            //draw filled part
            int position = 1;
            for (int i = 0; i <= onechunk * progress; i++)
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw unfilled part
            for (int i = position; i <= 31; i++)
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw totals
            Console.CursorLeft = 35;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(progress.ToString() + " of " + total.ToString() + "    "); //blanks at the end remove any excess
        }
    }
}
