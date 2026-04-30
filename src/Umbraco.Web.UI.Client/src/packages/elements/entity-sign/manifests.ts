import { UMB_ELEMENT_ENTITY_TYPE } from '../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entitySign',
		kind: 'icon',
		alias: 'Umb.EntitySign.Element.HasPendingChanges',
		name: 'Has Pending Changes Element Entity Sign',
		forEntityTypes: [UMB_ELEMENT_ENTITY_TYPE],
		forEntityFlags: ['Umb.PendingChanges'],
		meta: { iconName: 'icon-edit', label: '#content_unpublishedChanges' },
	},
	{
		type: 'entitySign',
		kind: 'icon',
		alias: 'Umb.EntitySign.Element.HasScheduledPublish',
		name: 'Element Has Scheduled Publish Entity Sign',
		forEntityTypes: [UMB_ELEMENT_ENTITY_TYPE],
		forEntityFlags: ['Umb.ScheduledForPublish'],
		overwrites: 'Umb.EntitySign.Element.HasPendingChanges',
		weight: 500,
		meta: {
			iconName: 'icon-time',
			label: '#content_scheduledPublishing',
			iconColorAlias: 'green',
		},
	},
];
