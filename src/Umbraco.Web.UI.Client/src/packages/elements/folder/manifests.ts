import { UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../entity.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import { UMB_ELEMENT_FOLDER_REPOSITORY_ALIAS } from './repository/constants.js';
import { UMB_ENTITY_IS_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'folderUpdate',
		alias: 'Umb.EntityAction.Element.Folder.Rename',
		name: 'Rename Element Folder Entity Action',
		forEntityTypes: [UMB_ELEMENT_FOLDER_ENTITY_TYPE],
		meta: {
			folderRepositoryAlias: UMB_ELEMENT_FOLDER_REPOSITORY_ALIAS,
		},
	},
	// TODO: [LK] Implement folder trash action
	// {
	// 	type: 'entityAction',
	// 	kind: 'folderTrash',
	// 	alias: 'Umb.EntityAction.Element.Folder.Trash',
	// 	name: 'Trash Element Folder Entity Action',
	// 	forEntityTypes: [UMB_ELEMENT_FOLDER_ENTITY_TYPE],
	// 	meta: {
	// 		icon: 'icon-trash',
	// 		label: '#actions_trash',
	// 		folderRepositoryAlias: UMB_ELEMENT_FOLDER_REPOSITORY_ALIAS,
	// 	},
	// 	conditions: [
	// 		{
	// 			alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
	// 		},
	// 	],
	// },
	{
		type: 'entityAction',
		kind: 'folderDelete',
		alias: 'Umb.EntityAction.Element.Folder.Delete',
		name: 'Delete Element Folder Entity Action',
		forEntityTypes: [UMB_ELEMENT_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-trash-empty',
			folderRepositoryAlias: UMB_ELEMENT_FOLDER_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_ENTITY_IS_TRASHED_CONDITION_ALIAS,
			},
		],
	},
	...repositoryManifests,
	...treeManifests,
	...workspaceManifests,
];
