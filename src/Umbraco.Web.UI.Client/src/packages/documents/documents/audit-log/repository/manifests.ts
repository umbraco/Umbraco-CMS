import { UMB_DOCUMENT_AUDIT_LOG_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_AUDIT_LOG_REPOSITORY_ALIAS,
		name: 'Document Audit Log Repository',
		api: () => import('./document-audit-log.repository.js'),
	},
];
