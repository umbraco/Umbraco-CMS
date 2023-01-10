import type { ManifestWorkspace } from '@umbraco-cms/models';

const workspaceAlias = 'Umb.Workspace.Dictionary';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: workspaceAlias,
	name: 'Dictionary Workspace',
	loader: () => import('./dictionary-workspace.element'),
	meta: {
		entityType: 'dictionary',
	},
};

export const manifests = [workspace];
