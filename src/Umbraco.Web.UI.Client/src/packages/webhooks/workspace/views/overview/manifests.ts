import type { ManifestWorkspace } from '@umbraco-cms/backoffice/extension-registry';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.WebhookRoot',
	name: 'Webhook Root Workspace',
	js: () => import('./webhook-overview-view.element.js'),
	meta: {
		entityType: 'webhook-root',
	},
};

export const manifests = [workspace];
