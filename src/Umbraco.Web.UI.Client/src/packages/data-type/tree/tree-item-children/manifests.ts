import { UMB_DATA_TYPE_FOLDER_ENTITY_TYPE, UMB_DATA_TYPE_ROOT_ENTITY_TYPE } from '../../entity.js';
import { manifests as collectionManifests } from './collection/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...collectionManifests,
	{
		type: 'entityAction',
		kind: 'reloadTreeItemChildren',
		alias: 'Umb.EntityAction.DataType.Tree.ReloadChildrenOf',
		name: 'Reload Data Type Tree Item Children Entity Action',
		forEntityTypes: [UMB_DATA_TYPE_ROOT_ENTITY_TYPE, UMB_DATA_TYPE_FOLDER_ENTITY_TYPE],
	},
];
