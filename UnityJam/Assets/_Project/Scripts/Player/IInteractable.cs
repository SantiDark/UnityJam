namespace Subject626
{
    /// <summary>Cualquier cosa del mundo con la que se interactua con E.</summary>
    public interface IInteractable
    {
        string Prompt();          // texto que muestra el HUD
        bool CanInteract();
        void Interact(PlayerInteractor by);
    }
}
