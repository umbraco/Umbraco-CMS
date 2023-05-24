import { manifests as menuManifests } from './menu.manifests.js';
import { manifests as templateManifests } from './templates/manifests.js';
import { manifests as stylesheetManifests } from './stylesheets/manifests.js';
import { manifests as partialManifests } from './partial-views/manifests.js';
import { manifests as modalManifests } from './modals/manifests.js';
import type { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';

import './components';
import './templates/components';

export const manifests = [
	...menuManifests,
	...templateManifests,
	...stylesheetManifests,
	...partialManifests,
	...modalManifests,
];

export const onInit: UmbEntryPointOnInit = (_host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);
};
