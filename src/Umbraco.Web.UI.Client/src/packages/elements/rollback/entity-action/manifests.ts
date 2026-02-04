import { UMB_ELEMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_USER_PERMISSION_ELEMENT_ROLLBACK } from '../../user-permissions/constants.js';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Element.Rollback',
		name: 'Element Rollback Entity Action',
		weight: 450,
		api: () => import('./rollback.action.js'),
		forEntityTypes: [UMB_ELEMENT_ENTITY_TYPE],
		meta: {
			icon: 'icon-history',
			label: '#actions_rollback',
			additionalOptions: true,
		},
		conditions: [
			{
				alias: 'Umb.Condition.UserPermission.Element',
				allOf: [UMB_USER_PERMISSION_ELEMENT_ROLLBACK],
			},
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
];
