using System.Collections.Generic;

public static class WhyIsThisNotDefaultCSharp {
    public static void Deconstruct<T, V>(this KeyValuePair<T, V> kvp, out T t, out V v) {
        t = kvp.Key;
        v = kvp.Value;
    }
}