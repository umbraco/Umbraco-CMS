import { UMB_EXTENSION_COLLECTION_ALIAS } from '../collection/manifests.js';
import { UMB_EXTENSION_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_EXTENSION_ROOT_WORKSPACE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'default',
		alias: UMB_EXTENSION_ROOT_WORKSPACE_ALIAS,
		name: 'Extension Insights Root Workspace',
		meta: {
			entityType: UMB_EXTENSION_ROOT_ENTITY_TYPE,
			headline: 'Extension Insights',
		},
	},
	{
		type: 'workspaceView',
		kind: 'collection',
		alias: 'Umb.WorkspaceView.Extension.Collection',
		name: 'Extension Insights Root Root Collection Workspace View',
		meta: {
			label: 'Collection',
			pathname: 'collection',
			icon: 'icon-layers',
			collectionAlias: UMB_EXTENSION_COLLECTION_ALIAS,
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_EXTENSION_ROOT_WORKSPACE_ALIAS,
			},
		],
	},
];
