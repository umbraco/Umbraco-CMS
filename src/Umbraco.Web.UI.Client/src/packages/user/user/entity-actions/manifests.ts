import { DISABLE_USER_REPOSITORY_ALIAS, USER_REPOSITORY_ALIAS } from '../repository/manifests.js';
import { UMB_USER_ENTITY_TYPE } from '../index.js';
import { UmbDisableUserEntityAction } from './disable/disable-user.action.js';
import { UmbDeleteEntityAction } from '@umbraco-cms/backoffice/entity-action';
import { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.User.Delete',
		name: 'Delete User Entity Action',
		weight: 900,
		api: UmbDeleteEntityAction,
		meta: {
			icon: 'umb:trash',
			label: 'Delete',
			repositoryAlias: USER_REPOSITORY_ALIAS,
			entityTypes: [UMB_USER_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.User.Disable',
		name: 'Disable User Entity Action',
		weight: 900,
		api: UmbDisableUserEntityAction,
		meta: {
			icon: 'umb:trash',
			label: 'Disable',
			repositoryAlias: DISABLE_USER_REPOSITORY_ALIAS,
			entityTypes: [UMB_USER_ENTITY_TYPE],
		},
	},
	/*
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.User.Disable',
		name: 'Disable User Entity Action',
		weight: 900,
		api: UmbChangeUserPasswordEntityAction,
		meta: {
			icon: 'umb:trash',
			label: 'Change Password',
			repositoryAlias: USER_REPOSITORY_ALIAS,
			entityTypes: [UMB_USER_ENTITY_TYPE],
		},
	},
  */
];

export const manifests = [...entityActions];
