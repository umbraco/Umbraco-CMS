import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as propertyEditorManifests } from './property-editors/manifests.js';
import { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';

import './components';

export const manifests = [...repositoryManifests, ...propertyEditorManifests];

export const onInit: UmbEntryPointOnInit = (host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);
};
