import { manifests as userGroupManifests } from './user-groups/manifests';
import { manifests as userManifests } from './users/manifests';
import { manifests as userSectionManifests } from './user-section/manifests';
import { manifests as currentUserManifests } from './current-user/manifests';

import { UmbCurrentUserStore, UMB_CURRENT_USER_STORE_CONTEXT_TOKEN } from './current-user/current-user.store';
import {
	UmbCurrentUserHistoryStore,
	UMB_CURRENT_USER_HISTORY_STORE_CONTEXT_TOKEN,
} from './current-user/current-user-history.store';
import { UmbUserItemStore } from './users/repository/user-item.store';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbEntrypointOnInit } from '@umbraco-cms/backoffice/extensions-api';

import './users/components';
import './user-groups/components';

export const manifests = [...userGroupManifests, ...userManifests, ...userSectionManifests, ...currentUserManifests];

export const onInit: UmbEntrypointOnInit = (host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);

	new UmbContextProviderController(host, UMB_CURRENT_USER_STORE_CONTEXT_TOKEN, new UmbCurrentUserStore());
	new UmbUserItemStore(host);
	new UmbContextProviderController(
		host,
		UMB_CURRENT_USER_HISTORY_STORE_CONTEXT_TOKEN,
		new UmbCurrentUserHistoryStore()
	);
};
