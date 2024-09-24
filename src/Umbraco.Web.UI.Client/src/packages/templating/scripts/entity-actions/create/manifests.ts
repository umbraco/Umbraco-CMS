import { UMB_SCRIPT_FOLDER_ENTITY_TYPE, UMB_SCRIPT_ROOT_ENTITY_TYPE } from '../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Script.CreateOptions',
		name: 'Script Create Options Entity Action',
		weight: 1200,
		api: () => import('./create.action.js'),
		forEntityTypes: [UMB_SCRIPT_ROOT_ENTITY_TYPE, UMB_SCRIPT_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-add',
			label: '#actions_create',
			additionalOptions: true,
		},
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.Script.CreateOptions',
		name: 'Script Create Options Modal',
		element: () => import('./options-modal/script-create-options-modal.element.js'),
	},
];
