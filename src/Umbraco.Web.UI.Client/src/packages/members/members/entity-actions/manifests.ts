import { MEMBER_REPOSITORY_ALIAS } from '../repository/manifests.js';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';
import { UmbDeleteEntityAction } from '@umbraco-cms/backoffice/entity-action';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Member.Delete',
		name: 'Delete Member Entity Action ',
		api: UmbDeleteEntityAction,
		meta: {
			icon: 'umb:trash',
			label: 'Delete',
			repositoryAlias: MEMBER_REPOSITORY_ALIAS,
			entityTypes: ['member'],
		},
	},
];

export const manifests = [...entityActions];
