import { UMB_MEDIA_AUDIT_LOG_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEDIA_AUDIT_LOG_REPOSITORY_ALIAS,
		name: 'Media Audit Log Repository',
		api: () => import('./media-audit-log.repository.js'),
	},
];
