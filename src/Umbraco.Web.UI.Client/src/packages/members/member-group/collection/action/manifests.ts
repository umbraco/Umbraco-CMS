import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const createManifest: ManifestTypes = {
	type: 'collectionAction',
	kind: 'button',
	name: 'Create Member Group Collection Action',
	alias: 'Umb.CollectionAction.MemberGroup.Create',
	weight: 200,
	meta: {
		label: 'Create',
		href: 'section/member-management/workspace/member-group/create',
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: 'Umb.Collection.MemberGroup',
		},
	],
};

export const manifests = [createManifest];
