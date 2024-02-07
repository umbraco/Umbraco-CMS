import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const createManifest: ManifestTypes = {
	type: 'collectionAction',
	kind: 'button',
	name: 'Create Member Collection Action',
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
};

export const manifests = [createManifest];
