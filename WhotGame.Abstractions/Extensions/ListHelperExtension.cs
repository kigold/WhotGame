namespace WhotGame.Abstractions.Extensions
{
    public static class ListHelperExtension
    {
        public static T Pop<T> (this List<T> list)
        {
            var card = list.Last();
            list.RemoveAt(list.Count - 1);
            return card;
        }
    }
}
