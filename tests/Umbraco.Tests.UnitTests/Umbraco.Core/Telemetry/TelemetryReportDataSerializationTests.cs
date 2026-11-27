using System.Text.Json.Nodes;
using NUnit.Framework;
using Umbraco.Cms.Core.Telemetry.Models;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Telemetry;

[TestFixture]
public class TelemetryReportDataSerializationTests
{
    [Test]
    public void Serialized_Payload_Includes_Schema_Version()
    {
        var serializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());
        var data = new TelemetryReportData { Id = Guid.NewGuid(), Version = "19.0.0" };

        JsonNode? json = JsonNode.Parse(serializer.Serialize(data));

        Assert.AreEqual(TelemetryReportData.CurrentSchemaVersion, json?["schemaVersion"]?.GetValue<int>());
        Assert.AreEqual("19.0.0", json?["version"]?.GetValue<string>());
    }
}
