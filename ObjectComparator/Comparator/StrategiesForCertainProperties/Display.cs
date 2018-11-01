namespace ObjectsComparator.Comparator.StrategiesForCertainProperties
{
    public sealed class Display
    {
        public string Expected { get; set; }
        public string Actually { get; set; }

        public Distinction GetDistinction<T>(T expected, T actual, string propertyName, string lambdaExpression)
        {
            var expectedValue = string.IsNullOrEmpty(Expected) ? ReplaceNull(expected) : Expected;
            var actuallyValue = string.IsNullOrEmpty(Actually) ? ReplaceNull(actual) : Actually;
            return new DistinctionForStrategy(lambdaExpression, propertyName, expectedValue, actuallyValue);
        }

        private static string ReplaceNull(object value) => value == null ? "null" : value.ToString();
    }
}