import {
	UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE,
	UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE,
} from '../../../entity.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'entityCreateOptionAction',
		alias: 'Umb.EntityCreateOptionAction.DocumentBlueprint.Default',
		name: 'Default Document Blueprint Entity Create Option Action',
		weight: 1000,
		api: () => import('./default-blueprint-create-option-action.js'),
		forEntityTypes: [UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE, UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-blueprint',
			label: 'Document Blueprint for',
			additionalOptions: true,
		},
	},
];
