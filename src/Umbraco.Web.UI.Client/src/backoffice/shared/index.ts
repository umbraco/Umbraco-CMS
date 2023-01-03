import { manifests as propertyActionManifests } from './property-actions/manifests';
import { manifests as propertyEditorModelManifests } from './property-editors/models/manifests';
import { manifests as propertyEditorUIManifests } from './property-editors/uis/manifests';
import { manifests as collectionBulkActionManifests } from './collection/bulk-actions/manifests';
import { manifests as collectionViewManifests } from './collection/views/manifests';

import { ManifestTypes, umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';

const registerExtensions = (manifests: Array<ManifestTypes>) => {
	manifests.forEach((manifest) => {
		if (umbExtensionsRegistry.isRegistered(manifest.alias)) return;
		umbExtensionsRegistry.register(manifest);
	});
};

registerExtensions([
	...propertyActionManifests,
	...propertyEditorModelManifests,
	...propertyEditorUIManifests,
	...collectionBulkActionManifests,
	...collectionViewManifests,
]);
