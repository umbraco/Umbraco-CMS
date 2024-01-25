import { UmbInviteUserCollectionAction } from './invite-user.collection-action.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const inviteManifest: ManifestTypes = {
	type: 'collectionAction',
	kind: 'button',
	name: 'Invite User Collection Action',
	alias: 'Umb.CollectionAction.User.Invite',
	api: UmbInviteUserCollectionAction,
	weight: 100,
	meta: {
		label: 'Invite',
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: 'Umb.Collection.User',
		},
	],
};

export const manifests = [inviteManifest];
