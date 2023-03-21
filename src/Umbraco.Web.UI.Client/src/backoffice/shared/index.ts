import { manifests as componentManifests } from './components';
import { manifests as propertyActionManifests } from './property-actions/manifests';
import { manifests as propertyEditorManifests } from './property-editors/manifests';
import { manifests as tinyMcePluginManifests } from './property-editors/uis/tiny-mce/plugins/manifests'; 
import { manifests as modalManifests } from './modals/manifests';

import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extensions-api';
import { ManifestTypes } from '@umbraco-cms/backoffice/extensions-registry';

export const manifests = [
	...componentManifests,
	...propertyActionManifests,
	...propertyEditorManifests,
	...tinyMcePluginManifests,
	...modalManifests,
];

const registerExtensions = (manifests: Array<ManifestTypes>) => {
	manifests.forEach((manifest) => umbExtensionsRegistry.register(manifest));
};

registerExtensions(manifests);
