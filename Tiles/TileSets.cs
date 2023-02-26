namespace JadeFables.Tiles {
    /// <summary>
    /// Class that acts as a psuedo-extension of vanilla's <seealso cref="TileID.Sets"/>,
    /// where-in the arrays in this class function for purely this mod's purposes.
    /// </summary>
    public static class TileSets {
        /// <summary>
        /// Whether or not a pre-defined TORCH tile (framing must be IDENTICAL to vanilla torches) has the capability
        /// of counting, and thus triggering, the Torch God event.
        /// </summary>
        public static bool[] TorchThatTriggersTorchGod = TileID.Sets.Factory.CreateBoolSet(false, TileID.Torches);
    }
}