import './components';

import { manifests as settingsSectionManifests } from './settings-section/manifests';
import { manifests as dashboardManifests } from './dashboards/manifests';
import { manifests as dataTypeManifests } from './data-types/manifests';
import { manifests as extensionManifests } from './extensions/manifests';
import { manifests as languageManifests } from './languages/manifests';
import { manifests as propertyActionManifests } from './property-actions/manifests';
import { manifests as propertyEditorModelManifests } from '../shared/property-editors/models/manifests';
import { manifests as propertyEditorUIManifests } from '../shared/property-editors/uis/manifests';
import { manifests as searchManifests } from '../search/manifests';
import { manifests as collectionBulkActionManifests } from './components/collection/bulk-actions/manifests';
import { manifests as collectionViewManifests } from './components/collection/views/manifests';

import { ManifestTypes, umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';

const registerExtensions = (manifests: Array<ManifestTypes>) => {
	manifests.forEach((manifest) => {
		if (umbExtensionsRegistry.isRegistered(manifest.alias)) return;
		umbExtensionsRegistry.register(manifest);
	});
};

registerExtensions([
	...settingsSectionManifests,
	...dashboardManifests,
	...dataTypeManifests,
	...extensionManifests,
	...languageManifests,
	...propertyActionManifests,
	...propertyEditorModelManifests,
	...propertyEditorUIManifests,
	...searchManifests,
	...collectionBulkActionManifests,
	...collectionViewManifests,
]);
