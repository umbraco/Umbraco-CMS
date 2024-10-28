import { UMB_USER_ENTITY_TYPE, UMB_USER_ROOT_ENTITY_TYPE } from '../../entity.js';

import { manifests as modalManifests } from './modal/manifests.js';

import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.User.Create',
		name: 'Create User Entity Action',
		weight: 1200,
		api: () => import('./create-user-entity-action.js'),
		forEntityTypes: [UMB_USER_ROOT_ENTITY_TYPE],
		meta: {
			icon: 'icon-add',
			label: '#actions_create',
			additionalOptions: true,
		},
	},
	{
		type: 'entityCreateOptionAction',
		kind: 'default',
		alias: 'Umb.EntityCreateOptionAction.User.Default',
		name: 'Default User Entity Create Option Action',
		weight: 1200,
		forEntityTypes: [UMB_USER_ENTITY_TYPE],
		meta: {
			icon: 'icon-add',
			label: 'Default (Extension)',
			additionalOptions: true,
		},
	},
	{
		type: 'entityCreateOptionAction',
		kind: 'default',
		alias: 'Umb.EntityCreateOptionAction.User.Api',
		name: 'Api User Entity Create Option Action',
		weight: 1200,
		forEntityTypes: [UMB_USER_ENTITY_TYPE],
		meta: {
			icon: 'icon-add',
			label: 'Api (Extension)',
			additionalOptions: true,
		},
	},
	...modalManifests,
];
