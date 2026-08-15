namespace Subject626
{
    /// <summary>Las seis salidas posibles. El juego se gana encontrandolas TODAS.</summary>
    public enum ExitId { Poster, FalseWall, KeyDoor, Panel, Plate, Keypad }

    public static class ExitInfo
    {
        public const int Count = 6;

        public static string Name(ExitId id)
        {
            switch (id)
            {
                case ExitId.Poster: return "POSTER del techo";
                case ExitId.FalseWall: return "PARED falsa";
                case ExitId.KeyDoor: return "PUERTA con la llave";
                case ExitId.Panel: return "PANEL a los golpes";
                case ExitId.Plate: return "COMPUERTA por peso";
                case ExitId.Keypad: return "TECLADO (codigo)";
            }
            return "salida";
        }
    }
}
