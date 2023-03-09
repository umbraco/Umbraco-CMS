import { manifests as settingsSectionManifests } from './section.manifests';
import { manifests as settingsMenuManifests } from './menu.manifests';
import { manifests as dashboardManifests } from './dashboards/manifests';
import { manifests as dataTypeManifests } from './data-types/manifests';
import { manifests as extensionManifests } from './extensions/manifests';
import { manifests as cultureManifests } from './cultures/manifests';
import { manifests as languageManifests } from './languages/manifests';
import { manifests as logviewerManifests } from './logviewer/manifests';

import { umbExtensionsRegistry } from '@umbraco-cms/extensions-api';
import { ManifestTypes } from '@umbraco-cms/extensions-registry';

export const manifests = [
	...settingsSectionManifests,
	...settingsMenuManifests,
	...dashboardManifests,
	...dataTypeManifests,
	...extensionManifests,
	...cultureManifests,
	...languageManifests,
	...logviewerManifests,
];

const registerExtensions = (manifests: Array<ManifestTypes>) => {
	manifests.forEach((manifest) => umbExtensionsRegistry.register(manifest));
};

registerExtensions(manifests);
