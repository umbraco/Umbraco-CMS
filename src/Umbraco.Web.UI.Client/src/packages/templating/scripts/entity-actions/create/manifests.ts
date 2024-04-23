import { UMB_SCRIPT_FOLDER_ENTITY_TYPE, UMB_SCRIPT_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UmbScriptCreateOptionsEntityAction } from './create.action.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Script.CreateOptions',
		name: 'Script Create Options Entity Action',
		weight: 1200,
		api: UmbScriptCreateOptionsEntityAction,
		forEntityTypes: [UMB_SCRIPT_ROOT_ENTITY_TYPE, UMB_SCRIPT_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-add',
			label: '#actions_create',
		},
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.Script.CreateOptions',
		name: 'Script Create Options Modal',
		element: () => import('./options-modal/script-create-options-modal.element.js'),
	},
];
