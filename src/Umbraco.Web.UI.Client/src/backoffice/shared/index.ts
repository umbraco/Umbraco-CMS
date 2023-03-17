import { manifests as componentManifests } from './components';
import { manifests as propertyActionManifests } from './property-actions/manifests';
import { manifests as propertyEditorManifests } from './property-editors/manifests';
import { manifests as collectionViewManifests } from './collection/views/manifests';
import { manifests as modalManifests } from './modals/manifests';
import type { UmbEntrypointOnInit } from '@umbraco-cms/extensions-api';
import { ManifestTypes } from '@umbraco-cms/extensions-registry';

export const manifests: Array<ManifestTypes> = [
	...componentManifests,
	...propertyActionManifests,
	...propertyEditorManifests,
	...collectionViewManifests,
	...modalManifests,
];

export const onInit: UmbEntrypointOnInit = (_host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);
};
