import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entitySign',
		kind: 'icon',
		alias: 'Umb.EntitySign.Document.HasScheduledPublish',
		name: 'Document has scheduled publish Entity Sign',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		// forEntityFlags: ['Umb.ScheduledForPublish'],
		// overwrites: 'Umb.EntitySign.Document.HasPendingChanges',
		weight: 500,
		meta: {
			iconName: 'icon-time',
			label: '#content_scheduledPublishing',
			iconColorAlias: 'green',
		},
	},
];
