import { UMB_LANGUAGE_ROOT_ENTITY_TYPE } from '../../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityCreateOptionAction',
		alias: 'Umb.EntityCreateOptionAction.Language.Default',
		name: 'Default Language Entity Create Option Action',
		weight: 1000,
		api: () => import('./default-language-create-option-action.js'),
		forEntityTypes: [UMB_LANGUAGE_ROOT_ENTITY_TYPE],
		meta: {
			icon: 'icon-globe',
			label: '#actions_create',
			additionalOptions: true,
		},
	},
];
