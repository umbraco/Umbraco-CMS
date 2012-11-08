using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Serialization;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class MacroTests
    {
        [Test]
        public void Can_Create_And_Serialize_Macro()
        {
            // Arrange
            var serviceStackSerializer = new ServiceStackJsonSerializer();
            var serializationService = new SerializationService(serviceStackSerializer);

            var macro = new Macro
                            {
                                Alias = "test",
                                CacheByPage = false,
                                CacheByMember = false,
                                DontRender = true,
                                Name = "Test",
                                Xslt = "/xslt/testMacro.xslt",
                                UseInEditor = false
                            };

            macro.Properties = new List<IMacroProperty>();
            macro.Properties.Add(new MacroProperty { Alias = "level", Name = "Level", SortOrder = 0, PropertyType = new Umbraco.Core.Macros.PropertyTypes.Number() });
            macro.Properties.Add(new MacroProperty { Alias = "fixedTitle", Name = "Fixed Title", SortOrder = 1, PropertyType = new Umbraco.Core.Macros.PropertyTypes.Text() });

            // Act
            var json = serializationService.ToStream(macro);
            string jsonString = json.ResultStream.ToJsonString();

            // Assert
            Assert.That(json, Is.Not.Null);
            Assert.That(json.Success, Is.True);
            Assert.That(jsonString, Is.Not.Empty);
            Assert.That(jsonString.StartsWith("{"), Is.True);
            Assert.That(jsonString.EndsWith("}"), Is.True);
        }

        [Test]
        public void Can_Create_And_Serialize_Then_Deserialize_Macro()
        {
            // Arrange
            var serviceStackSerializer = new ServiceStackJsonSerializer();
            var serializationService = new SerializationService(serviceStackSerializer);

            var macro = new Macro
                            {
                                Alias = "test",
                                CacheByPage = false,
                                CacheByMember = false,
                                DontRender = true,
                                Name = "Test",
                                Xslt = "/xslt/testMacro.xslt",
                                UseInEditor = false
                            };

            macro.Properties = new List<IMacroProperty>();
            macro.Properties.Add(new MacroProperty { Alias = "level", Name = "Level", SortOrder = 0, PropertyType = new Umbraco.Core.Macros.PropertyTypes.Number() });
            macro.Properties.Add(new MacroProperty { Alias = "fixedTitle", Name = "Fixed Title", SortOrder = 1, PropertyType = new Umbraco.Core.Macros.PropertyTypes.Text() });

            // Act
            var json = serializationService.ToStream(macro);
            string jsonString = json.ResultStream.ToJsonString();

            var deserialized = serializationService.FromJson<Macro>(jsonString);

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            object o = serializationService.FromStream(stream, typeof (Macro));
            var deserialized2 = o as IMacro;

            // Assert
            Assert.That(json.Success, Is.True);
            Assert.That(jsonString, Is.Not.Empty);
            Assert.That(deserialized, Is.Not.Null);
            Assert.That(deserialized2, Is.Not.Null);
            Assert.That(deserialized.Name, Is.EqualTo(macro.Name));
            Assert.That(deserialized.Alias, Is.EqualTo(macro.Alias));
            Assert.That(deserialized2.Name, Is.EqualTo(macro.Name));
            Assert.That(deserialized2.Alias, Is.EqualTo(macro.Alias));
        }

        [Test]
        public void Can_Write_Serialized_Macro_To_Disc()
        {
            // Arrange
            var serviceStackSerializer = new ServiceStackJsonSerializer();
            var serializationService = new SerializationService(serviceStackSerializer);
            var fileSystem = FileSystemProviderManager.Current.GetFileSystemProvider("macros");

            var macro = new Macro
                            {
                                Alias = "test",
                                CacheByPage = false,
                                CacheByMember = false,
                                DontRender = true,
                                Name = "Test",
                                Xslt = "/xslt/testMacro.xslt",
                                UseInEditor = false
                            };

            macro.Properties = new List<IMacroProperty>();
            macro.Properties.Add(new MacroProperty { Alias = "level", Name = "Level", SortOrder = 0, PropertyType = new Umbraco.Core.Macros.PropertyTypes.Number() });
            macro.Properties.Add(new MacroProperty { Alias = "fixedTitle", Name = "Fixed Title", SortOrder = 1, PropertyType = new Umbraco.Core.Macros.PropertyTypes.Text() });

            // Act
            var json = serializationService.ToStream(macro);
            string jsonString = json.ResultStream.ToJsonString();
            fileSystem.AddFile("test-serialized-Macro.macro", json.ResultStream, true);

            // Assert
            Assert.That(json.Success, Is.True);
            Assert.That(jsonString, Is.Not.Empty);
            Assert.That(fileSystem.FileExists("test-serialized-Macro.macro"), Is.True);
        }

        [Test]
        public void Can_Read_And_Deserialize_Macro_From_Disc()
        {
            // Arrange
            var serviceStackSerializer = new ServiceStackJsonSerializer();
            var serializationService = new SerializationService(serviceStackSerializer);
            var fileSystem = FileSystemProviderManager.Current.GetFileSystemProvider("macros");

            var macro = new Macro
                            {
                                Alias = "test",
                                CacheByPage = false,
                                CacheByMember = false,
                                DontRender = true,
                                Name = "Test",
                                Xslt = "/xslt/testMacro.xslt",
                                UseInEditor = false
                            };

            macro.Properties = new List<IMacroProperty>();
            macro.Properties.Add(new MacroProperty { Alias = "level", Name = "Level", SortOrder = 0, PropertyType = new Umbraco.Core.Macros.PropertyTypes.Number() });
            macro.Properties.Add(new MacroProperty { Alias = "fixedTitle", Name = "Fixed Title", SortOrder = 1, PropertyType = new Umbraco.Core.Macros.PropertyTypes.Text() });

            // Act
            var json = serializationService.ToStream(macro);
            string jsonString = json.ResultStream.ToJsonString();
            fileSystem.AddFile("test-serialized-Macro.macro", json.ResultStream, true);

            Stream stream = fileSystem.OpenFile("test-serialized-Macro.macro");
            object o = serializationService.FromStream(stream, typeof(Macro));
            var deserialized = o as IMacro;

            // Assert
            Assert.That(json.Success, Is.True);
            Assert.That(jsonString, Is.Not.Empty);
            Assert.That(fileSystem.FileExists("test-serialized-Macro.macro"), Is.True);
            Assert.That(deserialized, Is.Not.Null);
            Assert.That(deserialized.Name, Is.EqualTo(macro.Name));
            Assert.That(deserialized.Alias, Is.EqualTo(macro.Alias));
        }
    }
}