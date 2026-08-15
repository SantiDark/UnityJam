using UnityEngine;

namespace Subject626
{
    /// <summary>
    /// Teclado al lado de la puerta: abre el ingreso del codigo de 4 digitos.
    /// Si ya usaste esta salida, queda SELLADO (fuera de servicio).
    /// </summary>
    public class KeypadController : MonoBehaviour, IInteractable, IExit
    {
        public string code = "0000";
        bool sealedOff;

        public ExitId Id { get { return ExitId.Keypad; } }
        public bool Sealed { get { return sealedOff; } }

        public string Prompt()
        {
            return sealedOff ? "Teclado fuera de servicio" : "E  Usar teclado (codigo 4 digitos)";
        }
        public bool CanInteract() { return Game.State == GameState.Playing; }

        public void Interact(PlayerInteractor by)
        {
            if (sealedOff)
            {
                if (Game.Hud != null) Game.Hud.Toast("El teclado quedo fuera de servicio.");
                return;
            }
            if (KeypadUI.Instance != null) KeypadUI.Instance.Open(code);
        }

        public void ResetForRound(bool sealedOff)
        {
            this.sealedOff = sealedOff;
        }
    }
}
