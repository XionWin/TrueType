namespace Extension;
public static class SemanticExtension
{
    public static T With<T>(this T source, Action<T> action)
    {
        action?.Invoke(source);
        return source;
    }

}