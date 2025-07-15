import { UMB_MEDIA_ENTITY_TYPE, UMB_MEDIA_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Media.Create',
		name: 'Create Media Entity Action',
		weight: 1200,
		api: () => import('./create.action.js'),
		forEntityTypes: [UMB_MEDIA_ROOT_ENTITY_TYPE, UMB_MEDIA_ENTITY_TYPE],
		meta: {
			icon: 'icon-add',
			label: '#actions_create',
			additionalOptions: true,
		},
		conditions: [
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.Media.CreateOptions',
		name: 'Media Create Options Modal',
		element: () => import('./media-create-options-modal.element.js'),
	},
];
