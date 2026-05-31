import { UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE, UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE } from '../../../entity.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'entityCreateOptionAction',
		alias: 'Umb.EntityCreateOptionAction.PartialView.FromSnippet',
		name: 'From Snippet Partial View Entity Create Option Action',
		weight: 900,
		api: () => import('./from-snippet-create-option-action.js'),
		forEntityTypes: [UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE, UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-document-html',
			label: '#create_newPartialViewFromSnippet',
			additionalOptions: true,
		},
	},
];
