import { manifests as userGroupManifests } from './user-groups/manifests.js';
import { manifests as userManifests } from './users/manifests.js';
import { manifests as userSectionManifests } from './user-section/manifests.js';
import { manifests as currentUserManifests } from './current-user/manifests.js';

import {
	UmbCurrentUserHistoryStore,
	UMB_CURRENT_USER_HISTORY_STORE_CONTEXT_TOKEN,
} from './current-user/current-user-history.store.js';
import { UmbUserItemStore } from './users/repository/user-item.store.js';
import { UmbUserGroupItemStore } from './user-groups/repository/user-group-item.store.js';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';

import './users/components/index.js';
import './user-groups/components/index.js';

export const manifests = [...userGroupManifests, ...userManifests, ...userSectionManifests, ...currentUserManifests];

export const onInit: UmbEntryPointOnInit = (host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);

	new UmbUserItemStore(host);
	new UmbUserGroupItemStore(host);
	new UmbContextProviderController(
		host,
		UMB_CURRENT_USER_HISTORY_STORE_CONTEXT_TOKEN,
		new UmbCurrentUserHistoryStore()
	);
};
