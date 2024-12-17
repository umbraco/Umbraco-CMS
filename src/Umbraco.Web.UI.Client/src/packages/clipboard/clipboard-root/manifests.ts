import { UMB_CLIPBOARD_COLLECTION_ALIAS } from '../collection/index.js';
import { UMB_CLIPBOARD_ROOT_WORKSPACE_ALIAS } from './constants.js';
import { UMB_CLIPBOARD_ROOT_ENTITY_TYPE } from './entity.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'default',
		alias: UMB_CLIPBOARD_ROOT_WORKSPACE_ALIAS,
		name: 'Clipboard Root Workspace',
		meta: {
			entityType: UMB_CLIPBOARD_ROOT_ENTITY_TYPE,
			headline: 'Clipboard',
		},
	},
	{
		type: 'workspaceView',
		kind: 'collection',
		alias: 'Umb.WorkspaceView.ClipboardRoot.Collection',
		name: 'Clipboard Root Collection Workspace View',
		meta: {
			label: 'Entries',
			pathname: 'entries',
			icon: 'icon-layers',
			collectionAlias: UMB_CLIPBOARD_COLLECTION_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_CLIPBOARD_ROOT_WORKSPACE_ALIAS,
			},
		],
	},
];
