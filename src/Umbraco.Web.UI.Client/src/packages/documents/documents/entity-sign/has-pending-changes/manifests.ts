import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entitySign',
		kind: 'icon',
		alias: 'Umb.EntitySign.Document.HasPendingChanges',
		name: 'Has Pending Changes Document Entity Sign',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		forEntityFlags: ['Umb.PendingChanges'],
		meta: { iconName: 'icon-edit', label: '#content_unpublishedChanges' },
	},
];
