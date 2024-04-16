import {
	UMB_MEDIA_TYPE_ENTITY_TYPE,
	UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE,
	UMB_MEDIA_TYPE_ROOT_ENTITY_TYPE,
} from '../../entity.js';
import { UmbCreateMediaTypeEntityAction } from './create.action.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.MediaType.Create',
		name: 'Create Media Type Entity Action',
		weight: 1200,
		api: UmbCreateMediaTypeEntityAction,
		forEntityTypes: [UMB_MEDIA_TYPE_ENTITY_TYPE, UMB_MEDIA_TYPE_ROOT_ENTITY_TYPE, UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-add',
			label: '#actions_create',
		},
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.MediaTypeCreateOptions',
		name: 'Media Type Create Options Modal',
		element: () => import('./modal/media-type-create-options-modal.element.js'),
	},
];

export const manifests = [...entityActions];
