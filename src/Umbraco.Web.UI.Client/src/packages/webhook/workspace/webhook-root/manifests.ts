import type { ManifestTypes, ManifestWorkspace } from '@umbraco-cms/backoffice/extension-registry';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.WebhookRoot',
	name: 'Webhook Root Workspace',
	element: () => import('./webhook-root-workspace.element.js'),
	meta: {
		entityType: 'webhook-root',
	},
};

export const manifests: Array<ManifestTypes> = [workspace];
