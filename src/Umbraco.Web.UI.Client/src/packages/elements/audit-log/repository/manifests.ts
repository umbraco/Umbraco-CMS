import { UMB_ELEMENT_AUDIT_LOG_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_ELEMENT_AUDIT_LOG_REPOSITORY_ALIAS,
		name: 'Element Audit Log Repository',
		api: () => import('./element-audit-log.repository.js'),
	},
];
