export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'auditLogTrigger',
		alias: 'Umb.AuditLogTrigger.Core.Rollback',
		name: 'Core Audit Log Trigger: Rollback',
		forTriggerSource: 'Core',
		forTriggerOperation: 'Rollback',
		meta: { label: 'Rollback' },
	},
];
