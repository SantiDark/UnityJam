using System.Collections.Generic;
using UnityEngine;

namespace Subject626
{
    /// <summary>
    /// Gestor de la sala: recuerda la entrada y las poses iniciales de todo, y sabe
    /// "reiniciar" (troll de la puerta) o solo reubicar al jugador (caida al hueco).
    /// </summary>
    public class Room : MonoBehaviour
    {
        public Vector3 entrancePos;
        public float entranceYaw;

        public readonly List<Grabbable> grabbables = new List<Grabbable>();
        public RugCover rug;
        public KeyItem key;

        public void CaptureStarts()
        {
            foreach (Grabbable g in grabbables) if (g != null) g.CaptureStart();
            if (key != null) key.CaptureStart();
        }

        /// <summary>Objetos a su lugar y llave escondida (NO teletransporta).</summary>
        public void ResetProps()
        {
            if (Game.Carry != null) Game.Carry.ForceDrop();
            foreach (Grabbable g in grabbables) if (g != null) g.ResetPose();
            if (rug != null) rug.ResetState();
            if (key != null) key.ResetState();
        }

        /// <summary>Reset completo: objetos + jugador a la entrada.</summary>
        public void ResetRoom()
        {
            ResetProps();
            RespawnPlayer();
        }

        /// <summary>Solo reubica al jugador en la entrada (no toca los objetos).</summary>
        public void RespawnPlayer()
        {
            if (Game.Controller != null)
                Game.Controller.Teleport(entrancePos, entranceYaw);
        }
    }
}
