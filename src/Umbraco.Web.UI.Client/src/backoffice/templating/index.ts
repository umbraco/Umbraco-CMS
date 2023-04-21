import { manifests as menuManifests } from './menu.manifests';
import { manifests as templateManifests } from './templates/manifests';
import { manifests as stylesheetManifests } from './stylesheets/manifests';
import type { UmbEntrypointOnInit } from '@umbraco-cms/backoffice/extensions-api';

import './components';

export const manifests = [...menuManifests, ...templateManifests, ...stylesheetManifests];

export const onInit: UmbEntrypointOnInit = (_host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);
};
