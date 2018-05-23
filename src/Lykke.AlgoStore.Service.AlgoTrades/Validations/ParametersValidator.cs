namespace Lykke.AlgoStore.Service.AlgoTrades.Validations
{
    public static class ParametersValidator
    {
        public static bool ValidateMaxNumberOfAlgoInstances(int count)
        {
            return (count <= 1000) && (count >= 1);
        }
    }
}
