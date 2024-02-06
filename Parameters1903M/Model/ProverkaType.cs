using System.ComponentModel;

namespace Parameters1903M.Model
{
    public enum ProverkaType
    {
        [Description("Новое испытание")] New,
        [Description("Продолжение испытания")] Continue,
        [Description("Печать протокола")] Print,
        [Description("Расшифровывание и экспорт папки")] Export
    }
}
