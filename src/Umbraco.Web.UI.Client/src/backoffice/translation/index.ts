import { manifests as translationSectionManifests } from './section.manifest';
import { manifests as dictionaryManifests } from './dictionary/manifests';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extensions-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extensions-api';

export const manifests = [...translationSectionManifests, ...dictionaryManifests];

const registerExtensions = (manifests: Array<ManifestTypes>) => {
	manifests.forEach((manifest) => umbExtensionsRegistry.register(manifest));
};

registerExtensions(manifests);
