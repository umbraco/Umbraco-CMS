import { UMB_DOCUMENT_ENTITY_TYPE } from '@umbraco-cms/backoffice/document';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entitySign',
		kind: 'icon',
		alias: 'Umb.EntitySign.Document.RecentlyCreated',
		name: 'Recently Created Document Sign',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		element: () => import('./umb-recently-created-sign'),
		meta: {
			iconName: 'icon-umbraco',
		},
	},
];
