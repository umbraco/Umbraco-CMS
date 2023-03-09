import { manifests as mediaSectionManifests } from './section.manifests';
import { manifests as mediaMenuManifests } from './menu.manifests';
import { manifests as mediaManifests } from './media/manifests';
import { manifests as mediaTypesManifests } from './media-types/manifests';

import { umbExtensionsRegistry } from '@umbraco-cms/extensions-api';
import { ManifestTypes } from '@umbraco-cms/extensions-registry';

export const manifests = [...mediaSectionManifests, ...mediaMenuManifests, ...mediaManifests, ...mediaTypesManifests];

const registerExtensions = (manifests: Array<ManifestTypes>) => {
	manifests.forEach((manifest) => umbExtensionsRegistry.register(manifest));
};

registerExtensions(manifests);
