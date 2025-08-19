namespace SystemInfo.Core.Abstractions
{
    public interface IDiskDefragmenter
    {
        /// <summary>
        /// Executa otimização (SSD) ou desfragmentação (HDD) na unidade especificada.
        /// </summary>
        /// <param name="driveLetter">Letra da unidade (ex: "C")</param>
        /// <returns>Saída textual do defrag.exe</returns>
        string Optimize(string driveLetter);
    }
}
