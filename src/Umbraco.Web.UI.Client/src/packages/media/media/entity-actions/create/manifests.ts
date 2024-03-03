import { UMB_MEDIA_ENTITY_TYPE, UMB_MEDIA_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UmbCreateMediaEntityAction } from './create.action.js';
import type { ManifestModal, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Media.Create',
		name: 'Create Media Entity Action',
		weight: 1000,
		api: UmbCreateMediaEntityAction,
		meta: {
			icon: 'icon-add',
			label: 'Create',
			entityTypes: [UMB_MEDIA_ROOT_ENTITY_TYPE, UMB_MEDIA_ENTITY_TYPE],
		},
	},
];

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Media.CreateOptions',
		name: 'Media Create Options Modal',
		js: () => import('./media-create-options-modal.element.js'),
	},
];

export const manifests = [...entityActions, ...modals];
