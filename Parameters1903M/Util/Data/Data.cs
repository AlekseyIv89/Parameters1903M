using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Parameters1903M.Util.Data
{
    public static class Data
    {
        private static Encoding Encoding { get; } = Encoding.Unicode;

        /// <summary>
        /// Сохранение текста в файл
        /// </summary>
        /// <param name="text">Текстовое содержимое файла</param>
        /// <param name="fileName">Имя файла</param>
        /// Если false - записывает файл под указанным именем. При наличии файла с таким именем добавляет "_номер" к названию файла.
        /// Если true - перезаписывает файл вне зависмости от наличия файла с указанным именем.</param>
        public static async void Save(string text, string fileName, bool overwriteFile = true)
        {
            if (!Directory.Exists(GlobalVars.SavePath))
            {
                Directory.CreateDirectory(GlobalVars.SavePath);
            }

            string path = $@"{GlobalVars.SavePath}\{fileName}";

            if (!overwriteFile)
            {
                path = CheckFullFileName(GlobalVars.SavePath, fileName, false);
            }

            using (StreamWriter sw = new StreamWriter(path, false, Encoding))
            {
                //text = EncryptionDecryption.Encrypt(text);

                await sw.WriteAsync(text);
            }
        }

        /// <summary>
        /// Считывание содержимого файла
        /// </summary>
        /// <param name="filePath">Путь к файлу</param>
        /// <returns></returns>
        public static async Task<string> Read(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }

            string text;

            using (StreamReader sr = new StreamReader(filePath, Encoding))
            {
                text = await sr.ReadToEndAsync();
            }
            return text;
        }

        /// <summary>
        /// Возвращает полный путь к файлу для записи иди для чтения
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        /// <param name="isFileRead">Флаг, для получения имени файла для чтения</param>
        /// <returns></returns>
        public static string CheckFullFileName(string filePath, string fileName, bool isFileRead = true)
        {
            if (isFileRead)
            {
                string[] files = Directory.GetFiles($@"{filePath}\", $"{fileName}*");

                return files[files.Length - 1];
            }

            string fullPath = $@"{filePath}\{fileName}";

            //Проверка на нличие файла с таким именем и изменение имени файла
            FileInfo file = new FileInfo(fullPath);
            if (file.Exists)
            {
                int i = 2;
                FileInfo fileNew = new FileInfo($"{file.FullName}_{i}");
                while (fileNew.Exists)
                {
                    fileNew = new FileInfo($"{file.FullName}_{++i}");
                }
                fullPath = fileNew.FullName;
            }

            return fullPath;
        }
    }
}
