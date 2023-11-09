import { UmbCurrentUserContext } from './current-user/index.js';
import type { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';

export const onInit: UmbEntryPointOnInit = (host, extensionRegistry) => {
	new UmbCurrentUserContext(host);
};
