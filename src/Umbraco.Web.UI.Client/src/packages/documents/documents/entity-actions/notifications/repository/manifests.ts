import { UMB_DOCUMENT_NOTIFICATIONS_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_NOTIFICATIONS_REPOSITORY_ALIAS,
		name: 'Document Notifications Repository',
		api: () => import('./document-notifications.repository.js'),
	},
];
