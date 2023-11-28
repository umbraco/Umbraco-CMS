import { UMB_INVITE_USER_REPOSITORY_ALIAS } from '../repository/manifests.js';
import { UMB_USER_ENTITY_TYPE } from '../../index.js';
import { UmbResendInviteToUserEntityAction } from './resend-invite/resend-invite-to-user.action.js';
import { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.User.ResendInvite',
		name: 'Resend Invite User Entity Action',
		weight: 500,
		api: UmbResendInviteToUserEntityAction,
		meta: {
			icon: 'icon-message',
			label: 'Resend Invite',
			repositoryAlias: UMB_INVITE_USER_REPOSITORY_ALIAS,
			entityTypes: [UMB_USER_ENTITY_TYPE],
		},
		conditions: [
			{
				alias: 'Umb.Condition.User.AllowResendInviteAction',
			},
		],
	},
];

export const manifests = [...entityActions];
