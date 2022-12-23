import type { ManifestTree, ManifestWorkspace } from '@umbraco-cms/extensions-registry';

const alias = 'DocumentBlueprint';
const treeAlias = `Umb.Tree.${alias}`;
const rootWorkspaceAlias = `Umb.Workspace.${alias}.Root`;

const tree: ManifestTree = {
	type: 'tree',
	alias: treeAlias,
	name: 'Document Blueprints Tree',
	weight: 400,
	meta: {
		label: 'Document Blueprints',
		icon: 'umb:blueprint',
		sections: ['Umb.Section.Settings'],
		rootNodeEntityType: 'document-blueprint-root',
	},
};

const rootWorkspace: ManifestWorkspace = {
	type: 'workspace',
	alias: rootWorkspaceAlias,
	name: 'Document Blueprint Root Workspace',
	loader: () => import('./workspace/document-blueprint-root-workspace.element'),
	meta: {
		entityType: 'document-blueprint-root',
	},
};

export const manifests = [tree, rootWorkspace];
