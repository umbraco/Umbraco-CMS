import { manifests as workspaceManifests } from './workspace/manifests';
import type { ManifestTree } from '@umbraco-cms/extensions-registry';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.DocumentBlueprint',
	name: 'Document Blueprints Tree',
	weight: 400,
	meta: {
		label: 'Document Blueprints',
		icon: 'umb:blueprint',
		sections: ['Umb.Section.Settings'],
		rootNodeEntityType: 'document-blueprint-root',
	},
};

export const manifests = [tree, ...workspaceManifests];
