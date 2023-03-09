import { MEMBER_GROUP_REPOSITORY_ALIAS } from '../repository/manifests';
import { UmbDeleteEntityAction } from '@umbraco-cms/entity-action';
import { ManifestEntityAction } from 'libs/extensions-registry/entity-action.models';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.MemberGroup.Delete',
		name: 'Delete Member Group Entity Action ',
		meta: {
			entityType: 'member-group',
			icon: 'umb:trash',
			label: 'Delete',
			api: UmbDeleteEntityAction,
			repositoryAlias: MEMBER_GROUP_REPOSITORY_ALIAS,
		},
	},
];

export const manifests = [...entityActions];
