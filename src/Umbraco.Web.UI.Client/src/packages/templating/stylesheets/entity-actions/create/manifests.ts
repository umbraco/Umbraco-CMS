import { UMB_STYLESHEET_FOLDER_ENTITY_TYPE, UMB_STYLESHEET_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UmbStylesheetCreateOptionsEntityAction } from './create.action.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Stylesheet.CreateOptions',
		name: 'Stylesheet Create Options Entity Action',
		weight: 1000,
		api: UmbStylesheetCreateOptionsEntityAction,
		meta: {
			icon: 'icon-add',
			label: 'Create...',
			entityTypes: [UMB_STYLESHEET_ROOT_ENTITY_TYPE, UMB_STYLESHEET_FOLDER_ENTITY_TYPE],
		},
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.Stylesheet.CreateOptions',
		name: 'Stylesheet Create Options Modal',
		js: () => import('./options-modal/stylesheet-create-options-modal.element.js'),
	},
];
