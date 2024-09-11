import { UMB_USER_GROUP_WORKSPACE_PATH } from '../../paths.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const createManifest: ManifestTypes = {
	type: 'collectionAction',
	kind: 'button',
	name: 'Create User Group Collection Action',
	alias: 'Umb.CollectionAction.UserGroup.Create',
	weight: 200,
	meta: {
		label: '#general_create',
		href: `${UMB_USER_GROUP_WORKSPACE_PATH}/create`,
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: 'Umb.Collection.UserGroup',
		},
	],
};

export const manifests: Array<ManifestTypes> = [createManifest];
