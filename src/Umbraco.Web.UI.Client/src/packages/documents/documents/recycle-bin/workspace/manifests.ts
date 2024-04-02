import type { ManifestWorkspaces } from '@umbraco-cms/backoffice/extension-registry';

const workspace: ManifestWorkspaces = {
	type: 'workspace',
	kind: 'routable',
	alias: 'Umb.Workspace.Document.RecycleBin',
	name: 'Document Recycle Bin Workspace',
	api: () => import('./document-recycle-bin-workspace.context.js'),
	meta: {
		entityType: 'document-recycle-bin',
	},
};

export const manifests = [workspace];
