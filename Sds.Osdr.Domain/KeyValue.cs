namespace Sds.Osdr.Domain
{
    public class KeyValue<T>
    {
        public string Name { get; set; }
        public T Value { get; set; }
    }
}
