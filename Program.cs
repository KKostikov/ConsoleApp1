using System;
using System.IO;
using Newtonsoft.Json;

namespace ResRegV1mod
{
    class ResAreBusy : Exception { }
    class ResIdInvalid : Exception { }
    class UnRecommended : Exception { }
    class ResIsBusy : Exception { }
    class ResWasFree : Exception { }
    static class SetUp
    {
        public static string Path; //путь к файлу, сохраняющему модель
        private static void ClearModel() // создает модель
        {
            Console.WriteLine("Укажите количество ресурсов (больше 0):");
            try
            {
                Model.vRes_s = new Resrs[Convert.ToInt32(Console.ReadLine())];
                for (int i = 0; i < Model.vRes_s.Length; i++) {
                    Model.vRes_s[i] = new Resrs();
                    }
            }
            catch
            {
                Console.WriteLine("Введено некорректное число!");
                ClearModel();
            }
        }
        private static void GetModel() // считывает модель из файла
        {
            Console.WriteLine("Обновить файл? (Y/N)");
            if (Console.ReadLine().ToUpper() == "Y") ClearModel();
            else
            {
                string jsonString = File.ReadAllText(Path);
                Model.vRes_s = JsonConvert.DeserializeObject<Resrs[]>(jsonString);
                
            }
        }
        public static bool On()
        {
            try
            {
                if (File.Exists(Directory.GetCurrentDirectory() + @"\Resmod00.json"))
                {
                    Console.WriteLine("Использовать существующий стандартный файл Resmod00.json? (Y/N)");
                    if (Console.ReadLine().ToUpper() == "Y")
                    {
                        Path = Directory.GetCurrentDirectory() + @"\Resmod00.json";
                        GetModel();
                        return true;
                    }
                }
                else
                {
                    Console.WriteLine("Создать стандартный файл? (Y/N)");
                    if (Console.ReadLine().ToUpper() == "Y")
                    {
                        Path = Directory.GetCurrentDirectory() + @"\Resmod00.json";
                        ClearModel();
                        return true;
                    }
                };
                Console.WriteLine("Введите полный адрес нестандартного файла:");
                Path = Console.ReadLine();
                if (File.Exists(Path))
                {
                    GetModel();
                    return true;
                }
                else
                {
                    ClearModel();
                    return true;
                }
            }
            catch (IOException) { Console.WriteLine("Файл не открылся."); return false; }
            catch (Exception) { Console.WriteLine("Ошибка ввода-вывода."); return false; }
        }
    }
    static class Model
    {
        public static Resrs[] vRes_s;//Модель набора ресурсов
        public static void Occupy(string cn)
        {
            if ((Convert.ToInt16(cn) > vRes_s.Length) || (Convert.ToInt16(cn) <= 0)) throw new ResIdInvalid();
            if (vRes_s[Convert.ToInt16(cn) - 1].is_busy) throw new ResIsBusy();
            vRes_s[Convert.ToInt16(cn) - 1].is_busy = true;
        }
        public static void Free(string cn)
        {
            if ((Convert.ToInt16(cn) > vRes_s.Length) || (Convert.ToInt16(cn) <= 0)) throw new ResIdInvalid();
            if (!vRes_s[Convert.ToInt16(cn) - 1].is_busy) throw new ResWasFree();
            vRes_s[Convert.ToInt16(cn) - 1].is_busy = false;
        }
        public static string Request(string req_size) //возвращает номер первого свободного элемента необходимого размера в массиве, попавшегося при его переборе от начала
        {
            for (int i = 0; i < vRes_s.Length; i++)
            {
                if ((Convert.ToInt16(req_size) > 99) || (Convert.ToInt16(req_size) <= 0)) throw new ResIdInvalid();
                if (!vRes_s[i].is_busy && vRes_s[i].capacity>= Convert.ToInt16(req_size)) return Convert.ToString(i+1);
            }
            throw new ResAreBusy(); ;
        }
    }

     class Resrs {
        public int capacity;
        public bool is_busy;

        public override string ToString()
        {
            return capacity.ToString() + " ; " + is_busy.ToString(); 
        }
        public Resrs(int capacity = 30, bool is_busy = false)
        {
            this.capacity = capacity;
            this.is_busy = is_busy;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string Command;
            const string Help_string = "Перечень команд:\n REQUEST - запросить ресурс\n OCCUPY - занять ресурс\n FREE - освободить ресурс\n QUIT - выйти из программы\n PRINT - список ресурсов\n HELP - перечень команд";
            Console.WriteLine(Help_string);
            while (!SetUp.On()) ;
            do
            {
                try
                {
                    string jsonString = JsonConvert.SerializeObject(Model.vRes_s);
                    File.WriteAllText(SetUp.Path, jsonString);//сохранение модели
                }
                catch { Console.WriteLine("Не удалось сохранить файл"); }

                Console.WriteLine("Введите команду:");
                Command = Console.ReadLine();
                Command = Command.ToUpper();
                try
                {
                    if (Command == "REQUEST") { 
                        Console.WriteLine("Введите размер ресурса");
                        Console.WriteLine(Model.Request(Console.ReadLine()));
                    }
                    if (Command == "OCCUPY")
                    {
                        Console.WriteLine("Введите номер ресурса:");
                        Model.Occupy(Console.ReadLine());
                        Console.WriteLine("Ресурс стал занятым.");
                    };
                    if (Command == "FREE")
                    {
                        Console.WriteLine("Введите номер ресурса:");
                        Model.Free(Console.ReadLine());
                        Console.WriteLine("Ресурс освобождён.");
                    };
                    if(Command == "HELP") Console.WriteLine(Help_string);
                    if(Command == "PRINT")
                    {
                        for(int i =0; i<Model.vRes_s.Length; i++)
                        {
                            Console.WriteLine($"Ресурс {i+1}: " + Model.vRes_s[i]);
                        }
                    }
                }
                catch (OverflowException) { Console.WriteLine("Такого ресурса нет."); }
                catch (FormatException) { Console.WriteLine("Такого ресурса нет."); }
                catch (ResIdInvalid) { Console.WriteLine("Такого ресурса нет."); }
                catch (ResWasFree) { Console.WriteLine("Ресурс был свободен."); }
                catch (ResAreBusy) { Console.WriteLine("Все ресурсы заняты."); }
                catch (ResIsBusy) { Console.WriteLine("ресурс уже занят."); }
            }
            while (Command != "QUIT");
        }
    }
}
