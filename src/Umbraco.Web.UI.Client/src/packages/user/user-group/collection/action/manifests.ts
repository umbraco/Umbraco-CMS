import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const createManifest: ManifestTypes = {
	type: 'collectionAction',
	kind: 'button',
	name: 'Create User Group Collection Action',
	alias: 'Umb.CollectionAction.UserGroup.Create',
	weight: 200,
	meta: {
		label: 'Create',
		href: 'section/user-management/view/user-groups/user-group/create',
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: 'Umb.Collection.UserGroup',
		},
	],
};

export const manifests = [createManifest];
