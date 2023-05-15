import { manifests as repositoryManifests } from './repository/manifests';
import { manifests as propertyEditorManifests } from './property-editors/manifests';
import { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extensions-api';

import './components';

export const manifests = [...repositoryManifests, ...propertyEditorManifests];

export const onInit: UmbEntryPointOnInit = (host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);
};
