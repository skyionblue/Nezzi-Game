namespace OneWayTogether.Characters
{
    /// <summary>
    /// Implemented by any scene object that Dani can interact with:
    /// levers, switches, rope anchors, stackable zone drop points, etc.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Called when a character performs an Interact action while
        /// overlapping this object.
        /// </summary>
        /// <param name="source">The character that triggered the interaction.</param>
        void Interact(CharacterBase source);
    }
}
