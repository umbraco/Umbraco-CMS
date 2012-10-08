This build is from a fork which I've placed at https://bitbucket.org/boxbinary/nunit-2.5x
The only modification (so far) is to add the AllowPartiallyTrustedCallersAttribute to the CommonAssemblyInfo.cs to permit running assertions from within AppDomains that have partial trust permissionsets

- Alex Norcliffe, October 2011