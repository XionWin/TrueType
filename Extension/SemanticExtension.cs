namespace Extension;
public static class SemanticExtension
{
    public static T With<T>(this T source, Action<T> action)
    {
        action?.Invoke(source);
        return source;
    }

    public static TResult Then<T, TResult>(this T source, Func<T, TResult> function) =>
    function.Invoke(source);

    public static IEnumerator<T> Loop<T>(this IEnumerator<T> source, Func<T, bool> condition, Action<T> processing) =>
    condition(source.Current) ? source.With(x => processing(x.Current)).With(x => x.MoveNext()).Loop(condition, processing) : source;

    public static void Foreach<T>(this IEnumerable<T> items, Action<T, T?> handle) where T : struct
    {
        var current = default(T);
        T? pervious = null;

        foreach (var item in items)
        {
            current = item;
            handle?.Invoke(current, pervious);
            pervious = item;
        }
    }

}