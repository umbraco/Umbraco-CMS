import { manifests as workspaceManifests } from './workspace/manifests';
import type { ManifestTree } from '@umbraco-cms/extensions-registry';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.DocumentType',
	name: 'Document Types Tree',
	weight: 400,
	meta: {
		label: 'Document Types',
		icon: 'umb:folder',
		sections: ['Umb.Section.Settings'],
		storeContextAlias: 'umbDocumentTypeStore',
	},
};

export const manifests = [tree, ...workspaceManifests];
