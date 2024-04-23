using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Xml.Serialization;

public class TabData
{
    public string TabName { get; set; }
}

public class TabManager
{
    public static void SaveTabs(List<TabData> tabs, string filePath)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(List<TabData>));
        using (FileStream stream = new FileStream(filePath, FileMode.Create))
        {
            serializer.Serialize(stream, tabs);
        }
    }

    public static List<TabData> LoadTabs(string filePath)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(List<TabData>));
        using (FileStream stream = new FileStream(filePath, FileMode.Open))
        {
            return (List<TabData>)serializer.Deserialize(stream);
        }
    }
}
