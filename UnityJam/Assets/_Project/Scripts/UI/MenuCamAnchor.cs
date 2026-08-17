using UnityEngine;

namespace Subject626
{
    /// <summary>
    /// Marca desde donde mira la camara del menu principal (usa su posicion y rotacion).
    /// Como usarlo: crea un GameObject vacio en la escena, agregale este componente y encuadralo
    /// a gusto en la vista de escena (GameObject > Align With View lo pega a tu camara de escena).
    /// Si no hay ninguno, el menu usa un encuadre por defecto.
    /// </summary>
    public class MenuCamAnchor : MonoBehaviour { }
}
