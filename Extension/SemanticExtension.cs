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

}