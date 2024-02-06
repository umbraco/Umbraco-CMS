import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS } from '../../repository/index.js';
import { UmbTrashEntityAction } from '@umbraco-cms/backoffice/entity-action';

export const manifests = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Trash',
		name: 'Trash Document Entity Action',
		weight: 900,
		api: UmbTrashEntityAction,
		meta: {
			icon: 'icon-trash',
			label: 'Trash',
			repositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			entityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		},
		conditions: [
			{
				alias: 'Umb.Condition.UserPermission',
				match: 'Umb.UserPermission.Document.Delete',
			},
		],
	},
];
