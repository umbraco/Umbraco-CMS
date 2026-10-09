import { UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE, UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE } from '../../../entity.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'entityCreateOptionAction',
		alias: 'Umb.EntityCreateOptionAction.PartialView.Default',
		name: 'Default Partial View Entity Create Option Action',
		weight: 1000,
		api: () => import('./default-partial-view-create-option-action.js'),
		forEntityTypes: [UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE, UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-document-html',
			label: '#create_newEmptyPartialView',
			additionalOptions: true,
		},
	},
];
