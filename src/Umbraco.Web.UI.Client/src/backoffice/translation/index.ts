import { manifests as translationSectionManifests } from './section.manifest';
import { manifests as dictionaryManifests } from './dictionary/manifests';
import type { ManifestTypes } from '@umbraco-cms/models';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-api';

export const manifests = [...translationSectionManifests, ...dictionaryManifests];

const registerExtensions = (manifests: Array<ManifestTypes>) => {
	manifests.forEach((manifest) => umbExtensionsRegistry.register(manifest));
};

registerExtensions(manifests);
