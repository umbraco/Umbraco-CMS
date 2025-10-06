import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entitySign',
		kind: 'icon',
		alias: 'Umb.EntitySign.Document.HasSchedule',
		name: 'Has Schedule Document Entity Sign',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		forEntityFlags: ['Umb.ScheduledForPublish'],
		overwrites: 'Umb.EntitySign.Document.HasPendingChanges',
		weight: 500,
		meta: {
			iconName: 'icon-time',
			label: 'Scheduled publishing',
			iconColor: 'green',
		},
	},
];
