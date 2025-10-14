import { UMB_ENTITY_DATA_PICKER_ITEM_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_ENTITY_DATA_PICKER_ITEM_REPOSITORY_ALIAS,
		name: 'Entity Data Picker Item Repository',
		api: () => import('./entity-data-picker-item.repository.js'),
	},
];
