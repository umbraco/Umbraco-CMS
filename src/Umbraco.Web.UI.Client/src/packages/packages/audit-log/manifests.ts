export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'auditLogTrigger',
		alias: 'Umb.AuditLogTrigger.Core.PackageInstall',
		name: 'Core Audit Log Trigger: Package Install',
		forTriggerSource: 'Core',
		forTriggerOperation: 'PackageInstall',
		meta: { label: 'Package install' },
	},
];
