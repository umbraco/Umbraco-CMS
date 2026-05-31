import { UMB_SCRIPT_FOLDER_ENTITY_TYPE, UMB_SCRIPT_ROOT_ENTITY_TYPE } from '../../../entity.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'entityCreateOptionAction',
		alias: 'Umb.EntityCreateOptionAction.Script.Default',
		name: 'Default Script Entity Create Option Action',
		weight: 1000,
		api: () => import('./default-script-create-option-action.js'),
		forEntityTypes: [UMB_SCRIPT_ROOT_ENTITY_TYPE, UMB_SCRIPT_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-document-js',
			label: '#create_newJavascriptFile',
			additionalOptions: true,
		},
	},
];
