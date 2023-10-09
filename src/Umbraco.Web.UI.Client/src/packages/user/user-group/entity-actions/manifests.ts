import { USER_GROUP_REPOSITORY_ALIAS } from '../repository/manifests.js';
import { UMB_USER_GROUP_ENTITY_TYPE } from '../index.js';
import { UmbDeleteEntityAction } from '@umbraco-cms/backoffice/entity-action';
import { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.UserGroup.Delete',
		name: 'Delete User Group Entity Action',
		weight: 900,
		meta: {
			icon: 'umb:trash',
			label: 'Delete...',
			repositoryAlias: USER_GROUP_REPOSITORY_ALIAS,
			api: UmbDeleteEntityAction,
			entityTypes: [UMB_USER_GROUP_ENTITY_TYPE],
		},
	},
];

export const manifests = [...entityActions];
