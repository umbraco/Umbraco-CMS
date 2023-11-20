import { UMB_MEMBER_DETAIL_REPOSITORY_ALIAS } from '../repository/index.js';
import { UMB_MEMBER_ENTITY_TYPE } from '../entity.js';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';
import { UmbDeleteEntityAction } from '@umbraco-cms/backoffice/entity-action';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Member.Delete',
		name: 'Delete Member Entity Action ',
		api: UmbDeleteEntityAction,
		meta: {
			icon: 'icon-trash',
			label: 'Delete',
			repositoryAlias: UMB_MEMBER_DETAIL_REPOSITORY_ALIAS,
			entityTypes: [UMB_MEMBER_ENTITY_TYPE],
		},
	},
];

export const manifests = [...entityActions];
