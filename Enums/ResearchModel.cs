namespace StrategyGame.Enums
{
    public enum ResearchModel
    {
        //Only need to research on your capital city to unlock across the world
        OnlyOnCapital,
        //Researching on the capital makes it faster to research on other cities
        BoostedByCapital,
        //each city has their own tech tree
        PerCity
    }
}