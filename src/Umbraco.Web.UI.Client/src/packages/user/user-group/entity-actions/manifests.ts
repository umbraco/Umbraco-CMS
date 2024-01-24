import { UMB_USER_GROUP_REPOSITORY_ALIAS } from '../repository/manifests.js';
import { UMB_USER_GROUP_ENTITY_TYPE } from '../index.js';
import { UmbDeleteEntityAction } from '@umbraco-cms/backoffice/entity-action';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.UserGroup.Delete',
		name: 'Delete User Group Entity Action',
		weight: 900,
		api: UmbDeleteEntityAction,
		meta: {
			icon: 'icon-trash',
			label: 'Delete...',
			repositoryAlias: UMB_USER_GROUP_REPOSITORY_ALIAS,
			entityTypes: [UMB_USER_GROUP_ENTITY_TYPE],
		},
	},
];

export const manifests = [...entityActions];
