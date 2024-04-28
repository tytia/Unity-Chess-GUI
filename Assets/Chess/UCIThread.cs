namespace Chess {
    /// <summary>
    /// Represents a thread that runs the UCI protocol.
    /// In the future, to be able to host games between two engines, this class should be made non-static.
    /// </summary>
    public static class UCIThread {
        public static string engine_path { get; set; }
    }
}