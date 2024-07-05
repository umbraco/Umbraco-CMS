import { UMB_STYLESHEET_FOLDER_ENTITY_TYPE, UMB_STYLESHEET_ROOT_ENTITY_TYPE } from '../../entity.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Stylesheet.CreateOptions',
		name: 'Stylesheet Create Options Entity Action',
		weight: 1200,
		api: () => import('./create.action.js'),
		forEntityTypes: [UMB_STYLESHEET_ROOT_ENTITY_TYPE, UMB_STYLESHEET_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-add',
			label: '#actions_create',
		},
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.Stylesheet.CreateOptions',
		name: 'Stylesheet Create Options Modal',
		element: () => import('./options-modal/stylesheet-create-options-modal.element.js'),
	},
];
