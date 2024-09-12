import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const createManifest: ManifestTypes = {
	type: 'collectionAction',
	name: 'Create User Collection Action',
	alias: 'Umb.CollectionAction.User.Create',
	element: () => import('./create-user-collection-action.element.js'),
	weight: 200,
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: 'Umb.Collection.User',
		},
	],
};

export const manifests: Array<ManifestTypes> = [createManifest];
