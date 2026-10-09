import { UMB_TEMPLATE_ENTITY_TYPE, UMB_TEMPLATE_ROOT_ENTITY_TYPE } from '../../../entity.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'entityCreateOptionAction',
		alias: 'Umb.EntityCreateOptionAction.Template.Default',
		name: 'Default Template Entity Create Option Action',
		weight: 1000,
		api: () => import('./default-template-create-option-action.js'),
		forEntityTypes: [UMB_TEMPLATE_ENTITY_TYPE, UMB_TEMPLATE_ROOT_ENTITY_TYPE],
		meta: {
			icon: 'icon-document-html',
			label: '#create_newTemplate',
			additionalOptions: true,
		},
	},
];
