namespace System.Reflection
{
    public static class IntrospectionExtensions
    {
        public static TypeInfo GetTypeInfo(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            IReflectableType reflectableType = type as IReflectableType;
            if (reflectableType != null)
            {
                return reflectableType.GetTypeInfo();
            }
            return new TypeDelegator(type);
        }
    }
}