import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const createManifest: ManifestTypes = {
	type: 'collectionAction',
	name: 'Create Member Collection Action',
	kind: 'button',
	alias: 'Umb.CollectionAction.Member.Create',
	weight: 200,
	meta: {
		label: 'Create',
		href: 'section/member-management/workspace/member/create/member-type-1-id', // TODO: remove hardcoded member type id
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: 'Umb.Collection.Member',
		},
	],
	// element: () => import('./create-member-collection-action.element.js'),
};

export const manifests = [createManifest];
