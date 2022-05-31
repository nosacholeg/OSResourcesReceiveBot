using System.Management;

namespace OSResourcesReceiveBot
{
    /// <summary>
    /// Класс для получения ресурсов хостовой ОС
    /// </summary>
    class ResourcesOS
    {
        /// <summary>
        /// Метод для получения информации об ОС
        /// </summary>
        /// <returns>Информация об ОС</returns>
        public static string GetOSData()
        {
            string data = "Информация об ОС:\n";
            data += "Версия Windows:\n" + Environment.OSVersion + "\n";
            data += (Environment.Is64BitOperatingSystem ? "64" : "32") + " Bit операционная система\n";
            data += "Имя компьютера: " + Environment.MachineName + "\n";
            return data;
        }

        /// <summary>
        /// Метод для получения информации о центральном процессоре
        /// </summary>
        /// <returns>Информация о центральном процессоре</returns>
        public static string GetCPUData()
        {
            ManagementObjectSearcher searcher = new("SELECT * FROM Win32_Processor");
            string info = "Информация о процессорах:\n";

            int count = GetInfo(searcher, "Name").Count;
            string[] data = new string[count];

            for (int i = 0; i < count; i++)
            {
                data[i] += "\nПроцессор: " + (i + 1) + "\n";
                data[i] += "Имя процессора:\n" + GetInfo(searcher, "Name")[i] + "\n";
                data[i] += "Описание:\n" + GetInfo(searcher, "Description")[i] + "\n";
                data[i] += "Архитектура процессора: ";
                string str = GetInfo(searcher, "Architecture")[i];
                switch (str)
                {
                    case "0": data[i] += "x86"; break;
                    case "1": data[i] += "MIPS"; break;
                    case "2": data[i] += "Alpha"; break;
                    case "3": data[i] += "PowerPC"; break;
                    case "5": data[i] += "ARM"; break;
                    case "6": data[i] += "ia64"; break;
                    case "9": data[i] += "x64"; break;
                    case "12": data[i] += "ARM64"; break;
                }
                data[i] += "\n";
                data[i] += "Количество ядер:"+ GetInfo(searcher, "NumberOfCores")[i] + "\n";
                data[i] += "Количество потоков:"+ GetInfo(searcher, "ThreadCount")[i] + "\n";
                data[i] += "Текущая частота процессора:" + Convert.ToDouble(GetInfo(searcher, "CurrentClockSpeed")[i]) / 1000 + "GHz\n";
                data[i] += "Макс. частота процессора:" + Convert.ToDouble(GetInfo(searcher, "MaxClockSpeed")[i]) / 1000 + "GHz\n";
            }

            for (int i = 0; i < count; i++)
            {
                info += data[i];
            }

            return info;
        }
        
        /// <summary>
        /// Метод для получения информации о графическом процессоре
        /// </summary>
        /// <returns>Информация о графическом процессоре</returns>
        public static string GetVideoData()
        {
            string info = "Информация о видеоадаптерах\n";
            ManagementObjectSearcher searcher = new("SELECT * FROM Win32_VideoController");

            info += "Текущее разрешение:" + GetInfo(searcher, "CurrentHorizontalResolution")[0];
            info += "x" + GetInfo(searcher, "CurrentVerticalResolution")[0] + '\n';
            info += "Битов на пиксель: " + GetInfo(searcher, "CurrentBitsPerPixel")[0] + '\n';
            info += "Количество цветов: " + Math.Round(Convert.ToDouble(GetInfo(searcher, "CurrentNumberOfColors")[0]) / 1_000_000_000, 1) + " млрд\n";
            int count = GetInfo(searcher, "Name").Count;
            string[] data = new string[count];

            for (int i = 0; i < count; i++)
            {
                data[i] = "\nИнформация о видеоадаптере " + (i + 1) + ":\n";
                data[i] += "Имя процессора:\n" + GetInfo(searcher, "Name")[i] + '\n';
                data[i] += "Версия драйвера:\n" + GetInfo(searcher, "DriverVersion")[i] + '\n';
                data[i] += "Размер памяти видеоадаптера: " + Math.Round(Convert.ToDouble(GetInfo(searcher, "AdapterRAM")[i]) / 1024 / 1024 / 1024) + "Gb" + '\n';
                data[i] += "Тип архитектуры видео: ";
                switch (Convert.ToInt16(GetInfo(searcher, "VideoArchitecture")[i]))
                {
                    case 1: data[i] += "Другое"; break;
                    case 2: data[i] += "Неизвестно"; break;
                    case 3: data[i] += "CGA"; break;
                    case 4: data[i] += "EGA"; break;
                    case 5: data[i] += "VGA"; break;
                    case 6: data[i] += "SVGA"; break;
                    case 7: data[i] += "MDA"; break;
                    case 8: data[i] += "HGC"; break;
                    case 9: data[i] += "MCGA"; break;
                    case 10: data[i] += "8514A"; break;
                    case 11: data[i] += "XGA"; break;
                    case 12: data[i] += "Линейный буфер кадров"; break;
                    case 160: data[i] += "PC-98"; break;
                }
                data[i] += "\n";
            }

            for (int i = 0; i < count; i++)
            {
                info += data[i];
            }
            return info;
        }

        /// <summary>
        /// Метод для получения информации о накопителе
        /// </summary>
        /// <returns>Информация о накопителе</returns>
        public static string GetMemoryData()
        {
            string info = "Информация о памяти:\n";
            ManagementObjectSearcher searcher = new("SELECT * FROM Win32_DiskDrive");

            int count = GetInfo(searcher, "Caption").Count;
            string[] data = new string[count];

            for (int i = 0; i < count; i++)
            {
                data[i] += "\nИмя модуля памяти:\n" + GetInfo(searcher, "Caption")[i];
                data[i] += "Описание:\n" + GetInfo(searcher, "Description")[i];
                data[i] += "Размер накопителя: " + Math.Round(Convert.ToDouble(GetInfo(searcher, "Size")[i]) / 1024 / 1024 / 1024) + "Gb\n";
                data[i] += "Тип интерфейса:" + GetInfo(searcher, "InterfaceType")[i];
                data[i] += "Тип носителя:\n" + GetInfo(searcher, "MediaType")[i];
            }

            for (int i = 0; i < count; i++)
            {
                info += data[i];
            }

            info += "\nИнформация о разделах:";
            DriveInfo[] drives = DriveInfo.GetDrives();

            foreach (DriveInfo d in drives)
            {
                if (d.IsReady == true)
                {
                    info += "\n\nРаздел " + d.Name;
                    info += "\nTип диска: " + d.DriveType;
                    info += "\nОбъем памяти: " + Math.Round(Convert.ToDouble(d.TotalSize) / 1024 / 1024 / 1024, 3) + "Gb";
                    info += "\nOбъем свободного места: " + Math.Round(Convert.ToDouble(d.TotalFreeSpace) / 1024 / 1024 / 1024, 3) + "Gb";
                }
            }
            return info;
        }

        /// <summary>
        /// Метод для получения информации об оперативной(физической) памяти
        /// </summary>
        /// <returns>Информация об оперативной памяти</returns>
        public static string GetRAMData()
        {
            string info = "Информация об оперативной памяти:\n";
            ManagementObjectSearcher searcher = new("SELECT * FROM Win32_PhysicalMemory");
            int count = GetInfo(searcher, "Name").Count;
            string[] data = new string[count];

            for (int i = 0; i < count; i++)
            {
                data[i] = "\nИнформация о модуле памяти " + (i + 1) + ":\n";
                data[i] += "Тэг модуля памяти: " + GetInfo(searcher, "Tag")[i] + '\n';
                data[i] += "Емкость модуля памяти: " + Math.Round(Convert.ToDouble(GetInfo(searcher, "Capacity")[i]) / 1024 / 1024 / 1024) + "Gb" + '\n';
                data[i] += "Скорость модуля памяти: " + Math.Round(Convert.ToDouble(GetInfo(searcher, "Speed")[i]) / 1000, 1) + "GHz" + '\n';
                data[i] += "Изготовитель: " + GetInfo(searcher, "Manufacturer")[i] + '\n';

                data[i] += "Тип физической памяти: ";
                switch (Convert.ToInt16(GetInfo(searcher, "MemoryType")[i]))
                {
                    case 0: data[i] += "Неизвестно"; break;
                    case 1: data[i] += "Другое"; break;
                    case 2: data[i] += "DRAM"; break;
                    case 3: data[i] += "Синхронный DRAM"; break;
                    case 4: data[i] += "Кэш DRAM"; break;
                    case 5: data[i] += "EDO"; break;
                    case 6: data[i] += "EDRAM"; break;
                    case 7: data[i] += "VRAM"; break;
                    case 8: data[i] += "SRAM"; break;
                    case 9: data[i] += "ОЗУ"; break;
                    case 10: data[i] += "РОМ"; break;
                    case 11: data[i] += "Flash"; break;
                    case 12: data[i] += "EEPROM"; break;
                    case 13: data[i] += "FEPROM"; break;
                    case 14: data[i] += "EPROM"; break;
                    case 15: data[i] += "CDRAM"; break;
                    case 16: data[i] += "3DRAM"; break;
                    case 17: data[i] += "SDRAM"; break;
                    case 18: data[i] += "SGRAM"; break;
                    case 19: data[i] += "RDRAM"; break;
                    case 20: data[i] += "DDR"; break;
                    case 21: data[i] += "DDR2"; break;
                    case 22: data[i] += "DDR2 FB-DIMM"; break;
                    case 24: data[i] += "DDR3"; break;
                    case 25: data[i] += "FBD2"; break;
                    case 26: data[i] += "DDR4"; break;
                }
                data[i] += "\n";
            }

            for (int i = 0; i < count; i++)
            {
                info += data[i];
            }
            return info;
        }

        /// <summary>
        /// Метод для получение свойств для чтения из выборки класса
        /// </summary>
        /// <param name="searcher">Класс</param>
        /// <param name="property">Искомое свойство</param>
        /// <returns>Список свойств</returns>
        private static List<string> GetInfo(ManagementObjectSearcher searcher, string property)
        {
            List<string> result = new();
            try
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    result.Add(obj[property].ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return result;
        }
    }
}