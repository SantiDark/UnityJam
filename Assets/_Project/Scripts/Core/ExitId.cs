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

        public static string Text(ExitId id)
        {
            switch (id)
            {
                case ExitId.Poster: return "¿Por ahí arriba? ¿En serio? Hm. " +
                        "\nSupongo que nadie dijo que una salida tuviera que estar a la altura de tus ojos. Ingenioso.";

                case ExitId.FalseWall: return "Oh. Atravesaste la pared. [Pausa] No se suponía que vieras eso.";

                case ExitId.KeyDoor: return "Una llave debajo de la alfombra. Un clásico. " +
                        "\nTe sorprendería saber cuánta gente nunca piensa en mirar hacia abajo.";

                case ExitId.Panel: return "Oh. Violencia. Por supuesto. [Pausa] Bueno... funcionó. " +
                        "\nNo estoy seguro de qué dice eso sobre vos, pero funcionó.";

                case ExitId.Plate: return "Tres objetos, suficiente peso y ahí lo tenés. Física simple. " +
                        "\nBien hecho, 626.";

                case ExitId.Keypad: return "Ahh, encontraste el código. El 86% de los sujetos hizo exactamente lo mismo que vos. Interesante..." +
                        "\n Al parecer, la originalidad es estadísticamente poco común.";
            }

            return "";
        }

    }
}
