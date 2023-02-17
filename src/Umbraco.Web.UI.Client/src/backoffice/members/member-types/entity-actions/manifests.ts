import { UmbDeleteEntityAction } from '../../../../backoffice/shared/entity-actions/delete/delete.action';
import { MEMBER_TYPES_REPOSITORY_ALIAS } from '../repository/manifests';
import type { ManifestEntityAction } from '@umbraco-cms/models';

const entityType = 'member-type';
const repositoryAlias = MEMBER_TYPES_REPOSITORY_ALIAS;

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.MemberType.Delete',
		name: 'Delete Member Type Entity Action',
		weight: 100,
		meta: {
			entityType,
			icon: 'umb:trash',
			label: 'Delete',
			repositoryAlias,
			api: UmbDeleteEntityAction,
		},
	},
];

export const manifests = [...entityActions];
