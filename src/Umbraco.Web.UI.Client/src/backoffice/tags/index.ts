import { manifests as repositoryManifests } from './repository/manifests';
import { manifests as propertyEditorManifests } from './property-editors/manifests';
import { UmbEntrypointOnInit } from '@umbraco-cms/backoffice/extensions-api';

import './components';

export const manifests = [...repositoryManifests, ...propertyEditorManifests];

export const onInit: UmbEntrypointOnInit = (host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);
};
