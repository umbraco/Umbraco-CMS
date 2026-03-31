import { UMB_DOCUMENT_BLUEPRINT_AUDIT_LOG_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_BLUEPRINT_AUDIT_LOG_REPOSITORY_ALIAS,
		name: 'Document Blueprint Audit Log Repository',
		api: () => import('./document-blueprint-audit-log.repository.js'),
	},
];
