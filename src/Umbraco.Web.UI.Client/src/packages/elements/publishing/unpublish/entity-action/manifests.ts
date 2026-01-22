import { UMB_ELEMENT_ENTITY_TYPE } from '../../../entity.js';
import {
	UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
	UMB_USER_PERMISSION_ELEMENT_UNPUBLISH,
} from '../../../user-permissions/constants.js';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Element.Unpublish',
		name: 'Unpublish Element Entity Action',
		weight: 500,
		api: () => import('./unpublish.action.js'),
		forEntityTypes: [UMB_ELEMENT_ENTITY_TYPE],
		meta: {
			icon: 'icon-globe',
			label: '#actions_unpublish',
			additionalOptions: true,
		},
		conditions: [
			{
				alias: UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
				allOf: [UMB_USER_PERMISSION_ELEMENT_UNPUBLISH],
			},
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
];
