import type { ManifestWorkspace } from '@umbraco-cms/backoffice/extension-registry';

const workspaceAlias = 'Umb.Workspace.Webhooks';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: workspaceAlias,
	name: 'Webhook Root Workspace',
	js: () => import('./webhook-root-workspace.element.js'),
	meta: {
		entityType: 'webhook-root',
	},
};

export const manifests = [workspace];
