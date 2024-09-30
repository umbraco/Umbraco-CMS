export const UMB_DOCUMENT_NOTIFICATIONS_REPOSITORY_ALIAS = 'Umb.Repository.Document.Notifications';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_NOTIFICATIONS_REPOSITORY_ALIAS,
		name: 'Document Notifications Repository',
		api: () => import('./document-notifications.repository.js'),
	},
];
