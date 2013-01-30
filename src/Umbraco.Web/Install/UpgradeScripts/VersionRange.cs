using System;

namespace Umbraco.Web.Install.UpgradeScripts
{
    internal class VersionRange
    {
        private readonly Version _specificVersion;
        private readonly Version _startVersion;
        private readonly Version _endVersion;

        public VersionRange(Version specificVersion)
        {
            _specificVersion = specificVersion;
        }

        public VersionRange(Version startVersion, Version endVersion)
        {
            _startVersion = startVersion;
            _endVersion = endVersion;
        }

        /// <summary>
        /// Checks if the versionCheck is in the range (in between) the start and end version
        /// </summary>
        /// <param name="versionCheck"></param>
        /// <returns></returns>
        /// <remarks>
        /// For example if our version range is 4.10 -> 4.11.4, we want to return true if the version being checked is:
        /// greater than or equal to the start version but less than the end version.
        /// </remarks>
        public bool InRange(Version versionCheck)
        {
            //if it is a specific version
            if (_specificVersion != null)
                return versionCheck == _specificVersion;

            return versionCheck >= _startVersion && versionCheck < _endVersion;
        }
    }
}