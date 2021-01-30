public static class BehaviourTreeDrawer<T, D> where D : struct, BTCommand where T : struct {

    private static readonly IndentingStringBuilder cachedStringBuilder = new IndentingStringBuilder();

    public static void Draw(BT<T, D>.BehaviourTree tree) {
        cachedStringBuilder.Clear();
        tree.ShowAsString(cachedStringBuilder);
        EditorTools.Label(cachedStringBuilder.ToString());
    }
}