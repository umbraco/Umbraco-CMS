import { UMB_WEBHOOK_ENTITY_TYPE, UMB_WEBHOOK_WORKSPACE } from '../../entity.js';
import type {
	ManifestTypes,
	ManifestWorkspace,
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

export const manifests: Array<ManifestTypes> = [workspace, ...workspaceViews];
