import { manifests as auditLogActionManifests } from './audit-log-action/manifests.js';
import { manifests as infoAppManifests } from './info-app/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...auditLogActionManifests,
	...infoAppManifests,
	{
		type: 'auditLogTrigger',
		alias: 'Umb.AuditLogTrigger.Core',
		name: 'Core Audit Log Trigger',
		forTriggerSource: 'Core',
		meta: {
			labels: [
				{ operation: 'ScheduledPublish', label: 'Scheduled publish' },
				{ operation: 'Rollback', label: 'Rollback' },
				{ operation: 'Import', label: 'Imported' },
				{ operation: 'Export', label: 'Exported' },
				{ operation: 'PackageInstall', label: 'Package install' },
			],
		},
	},
];
