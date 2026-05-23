export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'auditLogTrigger',
		alias: 'Umb.AuditLogTrigger.Core.ScheduledPublish',
		name: 'Core Audit Log Trigger: Scheduled Publish',
		forTriggerSource: 'Core',
		forTriggerOperation: 'ScheduledPublish',
		meta: { label: 'Scheduled publish' },
	},
];
