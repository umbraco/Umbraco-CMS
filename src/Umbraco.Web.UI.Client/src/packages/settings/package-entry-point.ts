import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import type { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';
import {
	UMB_APP_LANGUAGE_CONTEXT_TOKEN,
	UmbAppLanguageContext,
} from './languages/app-language-select/app-language.context.js';
import { UmbThemeContext } from './themes/theme.context.js';
import { manifests } from './manifests.js';

export const onInit: UmbEntryPointOnInit = (host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);
	new UmbContextProviderController(host, UMB_APP_LANGUAGE_CONTEXT_TOKEN, new UmbAppLanguageContext(host));
	new UmbThemeContext(host);
};
