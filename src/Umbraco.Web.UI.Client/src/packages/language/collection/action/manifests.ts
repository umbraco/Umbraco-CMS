import { UmbCreateLanguageCollectionAction } from './create-language.collection-action.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const createManifest: ManifestTypes = {
	type: 'collectionAction',
	kind: 'button',
	name: 'Create Language Collection Action',
	alias: 'Umb.CollectionAction.Language.Create',
	api: UmbCreateLanguageCollectionAction,
	weight: 200,
	meta: {
		label: 'Create',
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: 'Umb.Collection.Language',
		},
	],
};

export const manifests = [createManifest];
