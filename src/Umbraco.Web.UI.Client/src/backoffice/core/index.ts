import { manifests as dashboardManifests } from './dashboards/manifests';
import { manifests as settingsSectionManifests } from './settings-section/manifests';
import { manifests as languageManifests } from './languages/manifests';

import { ManifestTypes, umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';

const registerExtensions = (manifests: Array<ManifestTypes> | Array<ManifestTypes>) => {
	manifests.forEach((manifest) => {
		if (umbExtensionsRegistry.isRegistered(manifest.alias)) return;
		umbExtensionsRegistry.register(manifest);
	});
};

registerExtensions([...settingsSectionManifests, ...dashboardManifests, ...languageManifests]);
