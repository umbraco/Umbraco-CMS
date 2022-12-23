import type { ManifestTree, ManifestWorkspace } from '@umbraco-cms/extensions-registry';

const alias = 'DocumentType';
const treeAlias = `Umb.Tree.${alias}`;
const workspaceAlias = `Umb.Workspace.${alias}`;

const tree: ManifestTree = {
	type: 'tree',
	alias: treeAlias,
	name: 'Document Types Tree',
	weight: 400,
	meta: {
		label: 'Document Types',
		icon: 'umb:folder',
		sections: ['Umb.Section.Settings'],
		storeContextAlias: 'umbDocumentTypeStore',
	},
};

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: workspaceAlias,
	name: 'Document Type Workspace',
	loader: () => import('./workspace/workspace-document-type.element'),
	meta: {
		entityType: 'document-type',
	},
};

export const manifests = [tree, workspace];
