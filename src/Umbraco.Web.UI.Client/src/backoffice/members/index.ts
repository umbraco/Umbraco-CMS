import { manifests as memberSectionManifests } from './section.manifests';
import { manifests as menuSectionManifests } from './menu.manifests';
import { manifests as memberGroupManifests } from './member-groups/manifests';
import { manifests as memberTypeManifests } from './member-types/manifests';
import { manifests as memberManifests } from './members/manifests';

import { umbExtensionsRegistry } from '@umbraco-cms/extensions-api';
import { ManifestTypes } from '@umbraco-cms/extensions-registry';

const registerExtensions = (manifests: Array<ManifestTypes>) => {
	manifests.forEach((manifest) => {
		if (umbExtensionsRegistry.isRegistered(manifest.alias)) return;
		umbExtensionsRegistry.register(manifest);
	});
};

registerExtensions([
	...memberSectionManifests,
	...menuSectionManifests,
	...memberGroupManifests,
	...memberTypeManifests,
	...memberManifests,
]);
