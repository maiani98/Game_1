namespace ChaosCosmos.Gameplay.GameLogic
{
    public enum GameState { Pregame, Playing, Paused, GameOver }

    public struct GameResultData
    {
        public float finalMass;
        public int xpEarned;
        public float timePlayed;
        // Aggiungere altri dati rilevanti se necessario (es. nemici sconfitti, etc.)
    }
}
