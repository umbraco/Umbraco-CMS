import {
	UMB_MEDIA_TYPE_ENTITY_TYPE,
	UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE,
	UMB_MEDIA_TYPE_ROOT_ENTITY_TYPE,
} from '../../index.js';
import { UMB_MEDIA_TYPE_DETAIL_REPOSITORY_ALIAS } from '../../repository/index.js';
import { UmbCreateMediaTypeEntityAction } from './create.action.js';
import { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.MediaType.Create',
		name: 'Create Media Type Entity Action',
		weight: 1000,
		api: UmbCreateMediaTypeEntityAction,
		meta: {
			icon: 'icon-add',
			label: 'Create...',
			repositoryAlias: UMB_MEDIA_TYPE_DETAIL_REPOSITORY_ALIAS,
			entityTypes: [UMB_MEDIA_TYPE_ENTITY_TYPE, UMB_MEDIA_TYPE_ROOT_ENTITY_TYPE, UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE],
		},
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.MediaTypeCreateOptions',
		name: 'Media Type Create Options Modal',
		js: () => import('./modal/media-type-create-options-modal.element.js'),
	},
];

export const manifests = [...entityActions];
