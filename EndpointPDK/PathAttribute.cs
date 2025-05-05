namespace EndpointPDK;

public class PathAttribute(string method, string path) : Attribute
{
    public string Path { get; set; } = path;
    public string Method { get; set; } = method;
}