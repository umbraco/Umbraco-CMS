import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const createManifest: ManifestTypes = {
	type: 'collectionAction',
	kind: 'button',
	name: 'Create Dictionary Collection Action',
	alias: 'Umb.CollectionAction.Dictionary.Create',
	weight: 200,
	meta: {
		label: 'Create',
		href: 'section/dictionary/workspace/dictionary/create/null',
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: 'Umb.Collection.Dictionary',
		},
	],
};

export const manifests = [createManifest];
