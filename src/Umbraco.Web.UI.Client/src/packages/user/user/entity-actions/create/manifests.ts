import { UMB_USER_ROOT_ENTITY_TYPE } from '../../entity.js';

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
	...modalManifests,
];
