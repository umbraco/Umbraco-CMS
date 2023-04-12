import { MEMBER_REPOSITORY_ALIAS } from '../repository/manifests';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/extensions-registry';
import { UmbDeleteEntityAction } from '@umbraco-cms/backoffice/entity-action';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Member.Delete',
		name: 'Delete Member Entity Action ',
		meta: {
			icon: 'umb:trash',
			label: 'Delete',
			api: UmbDeleteEntityAction,
			repositoryAlias: MEMBER_REPOSITORY_ALIAS,
		},
		conditions: {
			entityType: 'member',
		},
	},
];

export const manifests = [...entityActions];
