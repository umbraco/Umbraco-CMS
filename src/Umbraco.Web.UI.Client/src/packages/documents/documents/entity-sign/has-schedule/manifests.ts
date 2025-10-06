import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entitySign',
		kind: 'icon',
		alias: 'Umb.EntitySign.Document.HasSchedule',
		name: 'Has Schedule Document Entity Sign',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			iconName: 'icon-calendar',
			label: 'Scheduled publishing',
		},
	},
];
