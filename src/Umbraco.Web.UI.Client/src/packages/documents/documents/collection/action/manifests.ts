import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import type { ManifestCollectionAction } from '@umbraco-cms/backoffice/extension-registry';

export const createManifest: ManifestCollectionAction = {
	type: 'collectionAction',
	kind: 'button',
	name: 'Create Document Collection Action',
	alias: 'Umb.CollectionAction.Document.Create',
	element: () => import('./create-document-collection-action.element.js'),
	weight: 100,
	meta: {
		label: 'Create',

	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: 'Umb.Collection.Document',
		},
	],
};

export const manifests = [createManifest];
