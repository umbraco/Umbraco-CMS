import { UMB_COLLECTION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/collection';
import { UMB_IN_MODAL_CONDITION_ALIAS } from '@umbraco-cms/backoffice/modal';

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
			additionalOptions: true,
		},
		conditions: [
			{
				alias: UMB_COLLECTION_CONDITION_ALIAS,
				match: 'Umb.Collection.User',
			},
			{
				alias: UMB_IN_MODAL_CONDITION_ALIAS,
				match: false,
			},
		],
	},
];
