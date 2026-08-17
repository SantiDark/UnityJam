using UnityEngine;

namespace Subject626
{
    /// <summary>
    /// La puerta obvia. SIN llave: "reinicia" la sala (troll). CON llave: escapas de verdad.
    /// Si ya usaste esta salida antes, queda SELLADA (cerradura cambiada) y solo trollea.
    /// </summary>
    public class DoorController : MonoBehaviour, IInteractable, IExit
    {
        public Transform leaf;
        bool sealedOff;

        public ExitId Id { get { return ExitId.KeyDoor; } }

        public string Prompt()
        {
            if (Game.Ended) return "La puerta ya no lleva a ningun lado.";
            if (sealedOff) return "E  Puerta (cerradura cambiada)";
            return Game.HasKey ? "E  Abrir con la llave" : "E  Abrir puerta";
        }
        public bool CanInteract() { return Game.State == GameState.Playing; }

        public void Interact(PlayerInteractor by)
        {
            // Ya terminaste: la puerta es inerte. La unica salida real es que cierres el juego vos.
            if (Game.Ended) return;

            if (Game.HasKey && !sealedOff)
            {
                if (leaf != null) leaf.localRotation = Quaternion.Euler(0f, -95f, 0f);
                if (Game.Rounds != null) Game.Rounds.Escape(ExitId.KeyDoor);
                return;
            }

            // Sin llave (o sellada): la puerta no es la salida. Vuelve todo al principio.
            if (Game.Hud != null)
                Game.Hud.Toast(sealedOff
                    ? "Esa salida ya la usaste. Le cambiaron la cerradura."
                    : "La puerta no cede. Todo vuelve al principio. Proba OTRA cosa.");
            if (Game.Narrator != null) Game.Narrator.TryEnqueueDialogue(sealedOff ? "sealed" : "door_troll");
            if (Game.Rounds != null) Game.Rounds.SoftReset();
        }

        public void ResetForRound(bool sealedOff)
        {
            this.sealedOff = sealedOff;
            if (leaf != null) leaf.localRotation = Quaternion.identity;
        }
    }
}
