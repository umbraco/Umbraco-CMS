import { manifests as menuManifests } from './menu.manifests';
import { manifests as templateManifests } from './templates/manifests';
import { manifests as stylesheetManifests } from './stylesheets/manifests';
import { manifests as partialManifests } from './partial-views/manifests';
import { manifests as modalManifests } from './modals/manifests';
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
