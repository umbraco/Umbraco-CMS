import { UMB_DOCUMENT_ENTITY_TYPE } from '../../../entity.js';
import { UMB_USER_PERMISSION_DOCUMENT_PUBLISH } from '../../../user-permissions/document/constants.js';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Document.Publish',
		name: 'Publish Document Entity Action',
		weight: 600,
		api: () => import('./publish.action.js'),
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			icon: 'icon-globe',
			label: '#actions_publish',
			additionalOptions: true,
		},
		conditions: [
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_PUBLISH],
			},
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
];
