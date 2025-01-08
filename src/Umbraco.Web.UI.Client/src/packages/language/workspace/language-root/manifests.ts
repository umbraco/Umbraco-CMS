import { UMB_LANGUAGE_COLLECTION_ALIAS } from '../../collection/index.js';
import { UMB_LANGUAGE_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_LANGUAGE_ROOT_WORKSPACE_ALIAS } from './constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'default',
		alias: UMB_LANGUAGE_ROOT_WORKSPACE_ALIAS,
		name: 'Language Root Workspace',
		meta: {
			entityType: UMB_LANGUAGE_ROOT_ENTITY_TYPE,
			headline: '#treeHeaders_languages',
		},
	},
	{
		type: 'workspaceView',
		kind: 'collection',
		alias: 'Umb.WorkspaceView.LanguageRoot.Collection',
		name: 'Webhook Root Collection Workspace View',
		meta: {
			label: 'Collection',
			pathname: 'collection',
			icon: 'icon-layers',
			collectionAlias: UMB_LANGUAGE_COLLECTION_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_LANGUAGE_ROOT_WORKSPACE_ALIAS,
			},
		],
	},
];
