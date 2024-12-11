import { UMB_WEBHOOK_COLLECTION_ALIAS } from '../../constants.js';
import { UMB_WEBHOOK_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_WEBHOOK_ROOT_WORKSPACE_ALIAS } from './constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'default',
		alias: UMB_WEBHOOK_ROOT_WORKSPACE_ALIAS,
		name: 'Webhook Root Workspace',
		meta: {
			entityType: UMB_WEBHOOK_ROOT_ENTITY_TYPE,
			headline: '#treeHeaders_webhooks',
		},
	},
	{
		type: 'workspaceView',
		kind: 'collection',
		alias: 'Umb.WorkspaceView.WebhookRoot.Collection',
		name: 'Webhook Root Collection Workspace View',
		meta: {
			label: 'Collection',
			pathname: 'collection',
			icon: 'icon-layers',
			collectionAlias: UMB_WEBHOOK_COLLECTION_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_WEBHOOK_ROOT_WORKSPACE_ALIAS,
			},
		],
	},
];
