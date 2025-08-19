namespace SystemInfo.Core.Abstractions
{
    public interface ITempCleaner
    {
        /// <summary>
        /// Executa a limpeza de arquivos temporários e retorna o total liberado em MB.
        /// </summary>
        double CleanAll();
    }
}
