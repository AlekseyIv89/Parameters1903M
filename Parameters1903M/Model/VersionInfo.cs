using System;

namespace Parameters1903M.Model
{
    internal class VersionInfo
    {
        /// <summary>
        /// Версия программы
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Дата комиляции программы. Указываем вручную, чтобы при выпуске новой версии не извлекать дату из исходного файла
        /// </summary>
        public DateTime CompiledDate { get; set; }

        /// <summary>
        /// Изменения видимые в окне "Что нового"
        /// </summary>
        public string Changes { get; set; }

        /// <summary>
        /// Изменения, видимые только разработчику в окне "Что нового" в режиме Debug
        /// </summary>
        public string ChangesVisibleForDeveloper { get; set; }
    }
}
