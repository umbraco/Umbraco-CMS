import { UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE } from '../../../entity.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'entityCreateOptionAction',
		alias: 'Umb.EntityCreateOptionAction.MemberType.Default',
		name: 'Default Member Type Entity Create Option Action',
		weight: 1000,
		api: () => import('./default-member-type-create-option-action.js'),
		forEntityTypes: [UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE],
		meta: {
			icon: 'icon-user',
			label: '#content_membertype',
		},
	},
];
