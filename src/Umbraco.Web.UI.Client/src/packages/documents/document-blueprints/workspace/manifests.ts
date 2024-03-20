import type { ManifestWorkspace } from '@umbraco-cms/backoffice/extension-registry';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.DocumentBlueprint.Root',
	name: 'Document Blueprint Root Workspace',
	js: () => import('./document-blueprint-root-workspace.element.js'),
	meta: {
		entityType: 'document-blueprint-root',
	},
};

export const manifests = [workspace];
