import { UmbDocumentRepository } from '../repository/document.repository';
import type { ManifestTree } from '@umbraco-cms/models';

const treeAlias = 'Umb.Tree.Documents';

const tree: ManifestTree = {
	type: 'tree',
	alias: treeAlias,
	name: 'Documents Tree',
	meta: {
		repository: UmbDocumentRepository, // TODO: use alias instead of class
	},
};

export const manifests = [tree];
