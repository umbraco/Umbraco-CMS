import {
	UMB_MEMBER_TYPE_ENTITY_TYPE,
	UMB_MEMBER_TYPE_FOLDER_ENTITY_TYPE,
	UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE,
} from '../../entity.js';
import { UMB_MEMBER_TYPE_ROOT_WORKSPACE_ALIAS } from '../../member-type-root/constants.js';
import { UMB_MEMBER_TYPE_FOLDER_WORKSPACE_ALIAS } from '../folder/constants.js';
import { manifests as collectionManifests } from './collection/manifests.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'reloadTreeItemChildren',
		alias: 'Umb.EntityAction.MemberType.Tree.ReloadChildrenOf',
		name: 'Reload Member Type Tree Item Children Entity Action',
		forEntityTypes: [UMB_MEMBER_TYPE_ENTITY_TYPE, UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE, UMB_MEMBER_TYPE_FOLDER_ENTITY_TYPE],
	},
	{
		type: 'workspaceView',
		kind: 'collection',
		alias: 'Umb.WorkspaceView.MemberType.TreeItemChildrenCollection',
		name: 'Member Type Tree Item Children Collection Workspace View',
		meta: {
			label: 'Folder',
			pathname: 'folder',
			icon: 'icon-folder',
			collectionAlias: 'Umb.Collection.MemberType.TreeItemChildren',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				oneOf: [UMB_MEMBER_TYPE_ROOT_WORKSPACE_ALIAS, UMB_MEMBER_TYPE_FOLDER_WORKSPACE_ALIAS],
			},
		],
	},
	...collectionManifests,
];
