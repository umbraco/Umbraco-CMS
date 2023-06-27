import { UmbAppLanguageContext } from './languages/app-language-select/app-language.context.js';
import { UmbThemeContext } from './themes/theme.context.js';
import { manifests } from './manifests.js';
import type { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';

export const onInit: UmbEntryPointOnInit = (host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);

	// TODO: Move these to a extension type, that will new them up?
	new UmbAppLanguageContext(host);
	new UmbThemeContext(host);
};
