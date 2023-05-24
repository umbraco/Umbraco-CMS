import { UmbThemeContext } from './themes/theme.context.js';
import { manifests as settingsSectionManifests } from './section.manifests.js';
import { manifests as settingsMenuManifests } from './menu.manifests.js';
import { manifests as dashboardManifests } from './dashboards/manifests.js';
import { manifests as dataTypeManifests } from './data-types/manifests.js';
import { manifests as relationTypeManifests } from './relation-types/manifests.js';
import { manifests as extensionManifests } from './extensions/manifests.js';
import { manifests as cultureManifests } from './cultures/manifests.js';
import { manifests as languageManifests } from './languages/manifests.js';
import { manifests as logviewerManifests } from './logviewer/manifests.js';
import {
	UmbAppLanguageContext,
	UMB_APP_LANGUAGE_CONTEXT_TOKEN,
} from './languages/app-language-select/app-language.context.js';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import type { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';

import './cultures/components';
import './languages/components';

export const manifests = [
	...settingsSectionManifests,
	...settingsMenuManifests,
	...dashboardManifests,
	...dataTypeManifests,
	...extensionManifests,
	...cultureManifests,
	...languageManifests,
	...logviewerManifests,
	...relationTypeManifests,
];

export const onInit: UmbEntryPointOnInit = (host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);
	new UmbContextProviderController(host, UMB_APP_LANGUAGE_CONTEXT_TOKEN, new UmbAppLanguageContext(host));
	new UmbThemeContext(host);
};
