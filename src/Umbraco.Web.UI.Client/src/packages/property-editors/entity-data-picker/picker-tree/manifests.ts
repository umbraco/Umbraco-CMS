import { UMB_ENTITY_DATA_PICKER_TREE_ALIAS } from './constants.js';

const repositoryAlias = 'Umb.Repository.EntityDataPickerTree';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: repositoryAlias,
		name: 'Entity Data Picker Tree Repository',
		api: () => import('./entity-data-picker-tree.repository.js'),
	},
	{
		type: 'tree',
		kind: 'default',
		alias: UMB_ENTITY_DATA_PICKER_TREE_ALIAS,
		name: 'Entity Data Picker Tree',
		meta: {
			repositoryAlias,
		},
	},
];
