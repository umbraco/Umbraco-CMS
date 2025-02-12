import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
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
	},
];
