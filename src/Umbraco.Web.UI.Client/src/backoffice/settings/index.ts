import { manifests as settingsSectionManifests } from './section.manifests';
import { manifests as dashboardManifests } from './dashboards/manifests';
import { manifests as dataTypeManifests } from './data-types/manifests';
import { manifests as extensionManifests } from './extensions/manifests';
import { manifests as languageManifests } from './languages/manifests';
import { manifests as logviewerManifests } from './logviewer/manifests';

import { umbExtensionsRegistry } from '@umbraco-cms/extensions-api';
import { ManifestTypes } from '@umbraco-cms/extensions-registry';

const registerExtensions = (manifests: Array<ManifestTypes>) => {
	manifests.forEach((manifest) => {
		if (umbExtensionsRegistry.isRegistered(manifest.alias)) return;
		umbExtensionsRegistry.register(manifest);
	});
};

registerExtensions([
	...settingsSectionManifests,
	...dashboardManifests,
	...dataTypeManifests,
	...extensionManifests,
	...languageManifests,
	...logviewerManifests,
]);
