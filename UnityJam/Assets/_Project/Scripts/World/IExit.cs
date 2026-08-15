namespace Subject626
{
    /// <summary>
    /// Una salida que se puede resetear cada ronda del test. Si esta "sellada" (ya la usaste),
    /// se restaura pero queda INUTILIZABLE: la instalacion parchea el exploit que encontraste.
    /// </summary>
    public interface IExit
    {
        ExitId Id { get; }
        void ResetForRound(bool sealedOff);
    }
}
