import { UMB_WEBHOOK_ENTITY_TYPE, UMB_WEBHOOK_WORKSPACE } from '../../entity.js';
import { UmbSubmitWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type {
	ManifestTypes,
	ManifestWorkspace,
	ManifestWorkspaceActions,
	ManifestWorkspaceView,
} from '@umbraco-cms/backoffice/extension-registry';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	kind: 'routable',
	alias: UMB_WEBHOOK_WORKSPACE,
	name: 'Webhook Root Workspace',
	api: () => import('./webhook-workspace.context.js'),
	meta: {
		entityType: UMB_WEBHOOK_ENTITY_TYPE,
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Webhook.Details',
		name: 'Webhook Root Workspace Details View',
		js: () => import('./views/webhook-details-workspace-view.element.js'),
		weight: 300,
		meta: {
			label: 'Overview',
			pathname: 'overview',
			icon: 'icon-webhook',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
		],
	},
];

const workspaceActions: Array<ManifestWorkspaceActions> = [
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.Webhook.Save',
		name: 'Save Webhook Workspace Action',
		api: UmbSubmitWorkspaceAction,
		meta: {
			look: 'primary',
			color: 'positive',
			label: '#buttons_save',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
		],
	},
];

export const manifests: Array<ManifestTypes> = [workspace, ...workspaceViews, ...workspaceActions];
