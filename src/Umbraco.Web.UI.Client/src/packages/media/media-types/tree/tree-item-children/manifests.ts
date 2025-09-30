import {
	UMB_MEDIA_TYPE_ENTITY_TYPE,
	UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE,
	UMB_MEDIA_TYPE_ROOT_ENTITY_TYPE,
} from '../../entity.js';
import { UMB_MEDIA_TYPE_ROOT_WORKSPACE_ALIAS } from '../../media-type-root/constants.js';
import { UMB_MEDIA_TYPE_FOLDER_WORKSPACE_ALIAS } from '../folder/constants.js';
import { manifests as collectionManifests } from './collection/manifests.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'reloadTreeItemChildren',
		alias: 'Umb.EntityAction.MediaType.Tree.ReloadChildrenOf',
		name: 'Reload Media Type Tree Item Children Entity Action',
		forEntityTypes: [UMB_MEDIA_TYPE_ENTITY_TYPE, UMB_MEDIA_TYPE_ROOT_ENTITY_TYPE, UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE],
	},
	{
		type: 'workspaceView',
		kind: 'collection',
		alias: 'Umb.WorkspaceView.MediaType.TreeItemChildrenCollection',
		name: 'Media Type Tree Item Children Collection Workspace View',
		meta: {
			label: 'Folder',
			pathname: 'folder',
			icon: 'icon-folder',
			collectionAlias: 'Umb.Collection.MediaType.TreeItemChildren',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				oneOf: [UMB_MEDIA_TYPE_ROOT_WORKSPACE_ALIAS, UMB_MEDIA_TYPE_FOLDER_WORKSPACE_ALIAS],
			},
		],
	},
	...collectionManifests,
];
