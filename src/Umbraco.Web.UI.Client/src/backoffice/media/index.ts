import { manifests as mediaSectionManifests } from './section.manifests';
import { manifests as mediaManifests } from './media/manifests';
import { manifests as mediaTypesManifests } from './media-types/manifests';

import { ManifestTypes, umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';

const registerExtensions = (manifests: Array<ManifestTypes> | Array<ManifestTypes>) => {
	manifests.forEach((manifest) => {
		if (umbExtensionsRegistry.isRegistered(manifest.alias)) return;
		umbExtensionsRegistry.register(manifest);
	});
};

registerExtensions([...mediaSectionManifests, ...mediaManifests, ...mediaTypesManifests]);
