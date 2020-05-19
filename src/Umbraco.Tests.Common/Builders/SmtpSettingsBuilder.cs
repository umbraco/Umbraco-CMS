using Umbraco.Core.Configuration;

namespace Umbraco.Tests.Common.Builders
{
    public class SmtpSettingsBuilder : SmtpSettingsBuilder<object>
    {
        public SmtpSettingsBuilder() : base(null)
        {
        }
    }

    public class SmtpSettingsBuilder<TParent>
       : ChildBuilderBase<TParent, ISmtpSettings>
    {
        private string _from;
        private string _host;
        private int? _port;
        private string _pickupDirectoryLocation;
        private string _deliveryMethod;

        public SmtpSettingsBuilder(TParent parentBuilder) : base(parentBuilder)
        {
        }

        public SmtpSettingsBuilder<TParent> WithFrom(string from)
        {
            _from = from;
            return this;
        }

        public SmtpSettingsBuilder<TParent> WithHost(string host)
        {
            _host = host;
            return this;
        }

        public SmtpSettingsBuilder<TParent> WithPost(int port)
        {
            _port = port;
            return this;
        }

        public SmtpSettingsBuilder<TParent> WithPickupDirectoryLocation(string pickupDirectoryLocation)
        {
            _pickupDirectoryLocation = pickupDirectoryLocation;
            return this;
        }

        public SmtpSettingsBuilder<TParent> WithDeliveryMethod(string deliveryMethod)
        {
            _deliveryMethod = deliveryMethod;
            return this;
        }

        public override ISmtpSettings Build()
        {
            var from = _from ?? null;
            var host = _host ?? null;
            var port = _port ?? 25;
            var pickupDirectoryLocation = _pickupDirectoryLocation ?? null;
            var deliveryMethod = _deliveryMethod ?? null;

            return new TestSmtpSettings()
            {
                From = from,
                Host = host,
                Port = port,
                PickupDirectoryLocation = pickupDirectoryLocation,
                DeliveryMethod = deliveryMethod
            };
        }

        private class TestSmtpSettings : ISmtpSettings
        {
            public string From { get; set; }
            public string Host { get; set; }
            public int Port { get; set; }
            public string PickupDirectoryLocation { get; set; }
            public string DeliveryMethod { get; set; }
        }
    }
}
