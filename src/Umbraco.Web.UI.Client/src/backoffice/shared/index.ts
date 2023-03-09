import { manifests as componentManifests } from './components';
import { manifests as propertyActionManifests } from './property-actions/manifests';
import { manifests as propertyEditorManifests } from './property-editors/manifests';
import { manifests as collectionViewManifests } from './collection/views/manifests';
import { manifests as modalManifests } from './modals/manifests';

import { umbExtensionsRegistry } from '@umbraco-cms/extensions-api';
import { ManifestTypes } from '@umbraco-cms/extensions-registry';

export const manifests = [
	...componentManifests,
	...propertyActionManifests,
	...propertyEditorManifests,
	...collectionViewManifests,
	...modalManifests,
];

const registerExtensions = (manifests: Array<ManifestTypes>) => {
	manifests.forEach((manifest) => umbExtensionsRegistry.register(manifest));
};

registerExtensions(manifests);
