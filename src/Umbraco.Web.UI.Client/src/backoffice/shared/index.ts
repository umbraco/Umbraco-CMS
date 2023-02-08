import './components';

import { manifests as propertyActionManifests } from './property-actions/manifests';
import { manifests as propertyEditorModelManifests } from './property-editors/models/manifests';
import { manifests as propertyEditorUIManifests } from './property-editors/uis/manifests';
import { manifests as collectionViewManifests } from './collection/views/manifests';

import { umbExtensionsRegistry } from '@umbraco-cms/extensions-api';
import { ManifestTypes } from '@umbraco-cms/extensions-registry';

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
	...collectionViewManifests,
]);
