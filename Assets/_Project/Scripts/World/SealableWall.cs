using UnityEngine;

namespace Subject626
{
    /// <summary>
    /// Controla el sellado de la salida por la "pared falsa". Normalmente la pared no tiene
    /// collider (se cruza) y la plataforma tiene su salida. Si ya usaste esta salida, la
    /// instalacion le PONE collider a la pared: deja de ser falsa.
    /// </summary>
    public class SealableWall : MonoBehaviour, IExit
    {
        public Collider wallCollider;    // arranca deshabilitado (pared cruzable)
        public GameObject platformExit;  // ExitTrigger de la plataforma

        public ExitId Id { get { return ExitId.FalseWall; } }

        public void ResetForRound(bool sealedOff)
        {
            if (wallCollider != null) wallCollider.enabled = sealedOff;
            if (platformExit != null)
            {
                ExitTrigger et = platformExit.GetComponent<ExitTrigger>();
                if (et != null) et.ResetState();
                platformExit.SetActive(!sealedOff);
            }
        }
    }
}
