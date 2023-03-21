import { manifests as searchManifests } from '../search/manifests';

import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extensions-api';
import { ManifestTypes } from '@umbraco-cms/backoffice/extensions-registry';

export const manifests = [...searchManifests];

const registerExtensions = (manifests: Array<ManifestTypes>) => {
	manifests.forEach((manifest) => umbExtensionsRegistry.register(manifest));
};

registerExtensions(manifests);
