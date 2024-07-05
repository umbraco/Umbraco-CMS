import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const inviteManifest: ManifestTypes = {
	type: 'collectionAction',
	kind: 'button',
	name: 'Invite User Collection Action',
	alias: 'Umb.CollectionAction.User.Invite',
	api: () => import('./invite-user.collection-action.js'),
	weight: 100,
	meta: {
		label: '#user_invite',
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: 'Umb.Collection.User',
		},
	],
};

export const manifests: Array<ManifestTypes> = [inviteManifest];
