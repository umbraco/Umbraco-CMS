import { UMB_DATA_TYPE_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { UMB_DATA_TYPE_ROOT_WORKSPACE_ALIAS } from '../../data-type-root/index.js';
import { UMB_DATA_TYPE_FOLDER_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { UMB_DATA_TYPE_FOLDER_WORKSPACE_ALIAS } from './workspace/index.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'folderUpdate',
		alias: 'Umb.EntityAction.DataType.Folder.Rename',
		name: 'Rename Data Type Folder Entity Action',
		forEntityTypes: [UMB_DATA_TYPE_FOLDER_ENTITY_TYPE],
		meta: {
			folderRepositoryAlias: UMB_DATA_TYPE_FOLDER_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'entityAction',
		kind: 'folderDelete',
		alias: 'Umb.EntityAction.DataType.Folder.Delete',
		name: 'Delete Data Type Folder Entity Action',
		forEntityTypes: [UMB_DATA_TYPE_FOLDER_ENTITY_TYPE],
		meta: {
			folderRepositoryAlias: UMB_DATA_TYPE_FOLDER_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'workspaceView',
		kind: 'collection',
		alias: 'Umb.WorkspaceView.DataType.TreeItemChildrenCollection',
		name: 'Data Type Tree Item Children Collection Workspace View',
		meta: {
			label: 'Folder',
			pathname: 'folder',
			icon: 'icon-folder',
			collectionAlias: 'Umb.Collection.DataType.TreeItemChildren',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				oneOf: [UMB_DATA_TYPE_ROOT_WORKSPACE_ALIAS, UMB_DATA_TYPE_FOLDER_WORKSPACE_ALIAS],
			},
		],
	},
	...repositoryManifests,
	...workspaceManifests,
];
