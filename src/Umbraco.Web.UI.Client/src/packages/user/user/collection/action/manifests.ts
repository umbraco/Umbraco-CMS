import { UmbCreateUserCollectionAction } from './create-user.collection-action.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const createManifest: ManifestTypes = {
	type: 'collectionAction',
	kind: 'button',
	name: 'Create User Collection Action',
	alias: 'Umb.CollectionAction.User.Create',
	api: UmbCreateUserCollectionAction,
	weight: 200,
	meta: {
		label: 'Create',
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: 'Umb.Collection.User',
		},
	],
};

export const manifests = [createManifest];
