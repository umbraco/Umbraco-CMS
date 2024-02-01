import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const createManifest: ManifestTypes = {
	type: 'collectionAction',
	kind: 'button',
	name: 'Create Language Collection Action',
	alias: 'Umb.CollectionAction.Language.Create',
	weight: 200,
	meta: {
		label: 'Create',
		href: 'section/settings/workspace/language/create',
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: 'Umb.Collection.Language',
		},
	],
};

export const manifests = [createManifest];
