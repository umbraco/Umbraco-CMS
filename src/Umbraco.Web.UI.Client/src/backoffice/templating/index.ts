import { manifests as menuManifests } from './menu.manifests';
import { manifests as templateManifests } from './templates/manifests';
import { manifests as stylesheetManifests } from './stylesheets/manifests';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extensions-api';
import { ManifestTypes } from '@umbraco-cms/backoffice/extensions-registry';

import './components';

export const manifests = [...menuManifests, ...templateManifests, ...stylesheetManifests];

const registerExtensions = (manifests: Array<ManifestTypes>) => {
	manifests.forEach((manifest) => umbExtensionsRegistry.register(manifest));
};

registerExtensions(manifests);
