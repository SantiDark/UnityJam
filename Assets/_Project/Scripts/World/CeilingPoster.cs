using UnityEngine;

namespace Subject626
{
    /// <summary>
    /// Poster del techo: solo se saca estando ELEVADO (apilando cajas). Al sacarlo, escapas.
    /// Si ya usaste esta salida, queda SELLADO (atornillado) y no se puede sacar.
    /// </summary>
    public class CeilingPoster : MonoBehaviour, IInteractable, IExit
    {
        public Transform vent;
        public float minFeetHeight = 1.1f;
        bool sealedOff;

        public ExitId Id { get { return ExitId.Poster; } }

        bool PlayerElevated()
        {
            if (Game.Player == null) return false;
            return Game.Player.position.y > minFeetHeight;
        }

        public string Prompt()
        {
            if (sealedOff) return "Poster atornillado (ya usaste esta salida)";
            return PlayerElevated() ? "E  Sacar el poster" : "Esta muy alto... subite a algo";
        }
        public bool CanInteract() { return true; }

        public void Interact(PlayerInteractor by)
        {
            if (sealedOff)
            {
                if (Game.Hud != null) Game.Hud.Toast("El poster ahora esta atornillado.");
                return;
            }
            if (!PlayerElevated())
            {
                if (Game.Hud != null) Game.Hud.Toast("No llegas. Apila cajas y subite.");
                return;
            }
            gameObject.SetActive(false);
            if (vent != null) vent.gameObject.SetActive(true);
            if (Game.Rounds != null) Game.Rounds.Escape(ExitId.Poster);
        }

        public void ResetForRound(bool sealedOff)
        {
            this.sealedOff = sealedOff;
            gameObject.SetActive(true);          // el poster vuelve a estar
            if (vent != null) vent.gameObject.SetActive(false);
        }
    }
}
