import { UMB_ENTITY_DATA_PICKER_COLLECTION_MENU_ALIAS } from './constants.js';

const collectionRepositoryAlias = 'Umb.Repository.EntityDataPickerCollection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: collectionRepositoryAlias,
		name: 'Entity Data Picker Collection Repository',
		api: () => import('./entity-data-collection.repository.js'),
	},
	{
		type: 'collectionMenu',
		kind: 'default',
		alias: UMB_ENTITY_DATA_PICKER_COLLECTION_MENU_ALIAS,
		name: 'Entity Data Picker Collection Menu',
		meta: {
			collectionRepositoryAlias,
		},
	},
];
