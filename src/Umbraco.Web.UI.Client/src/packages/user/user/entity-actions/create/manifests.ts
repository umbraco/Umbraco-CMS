import { UMB_USER_ROOT_ENTITY_TYPE } from '../../entity.js';
import { manifests as apiUser } from './api-user/manifests.js';
import { manifests as defaultUser } from './default-user/manifests.js';
import { manifests as modalManifests } from './modal/manifests.js';

import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'entityAction',
		kind: 'create',
		alias: 'Umb.EntityAction.User.Create',
		name: 'Create User Entity Action',
		forEntityTypes: [UMB_USER_ROOT_ENTITY_TYPE],
	},
	...apiUser,
	...defaultUser,
	...modalManifests,
];
