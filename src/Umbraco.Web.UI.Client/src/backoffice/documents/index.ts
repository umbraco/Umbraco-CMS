import { manifests as documentBlueprintManifests } from './document-blueprints/manifests';
import { manifests as documentTypeManifests } from './document-types/manifests';

import { ManifestTypes, umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';

const registerExtensions = (manifests: Array<ManifestTypes> | Array<ManifestTypes>) => {
	manifests.forEach((manifest) => {
		if (umbExtensionsRegistry.isRegistered(manifest.alias)) return;
		umbExtensionsRegistry.register(manifest);
	});
};

registerExtensions([...documentBlueprintManifests, ...documentTypeManifests]);
