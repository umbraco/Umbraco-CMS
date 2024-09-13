import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestCollectionAction } from '@umbraco-cms/backoffice/collection';

export const createManifest: ManifestCollectionAction = {
	type: 'collectionAction',
	kind: 'button',
	name: 'Create Document Collection Action',
	alias: 'Umb.CollectionAction.Document.Create',
	element: () => import('./create-document-collection-action.element.js'),
	weight: 100,
	meta: {
		label: '#general_create',
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: 'Umb.Collection.Document',
		},
	],
};

export const manifests: Array<ManifestTypes> = [createManifest];
