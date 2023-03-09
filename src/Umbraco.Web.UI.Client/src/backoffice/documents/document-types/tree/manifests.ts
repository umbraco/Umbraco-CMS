import { UmbDocumentTypeRepository } from '../repository/document-type.repository';
import type { ManifestTree } from '@umbraco-cms/models';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.DocumentTypes',
	name: 'Document Types Tree',
	meta: {
		repository: UmbDocumentTypeRepository,
	},
};

export const manifests = [tree];
