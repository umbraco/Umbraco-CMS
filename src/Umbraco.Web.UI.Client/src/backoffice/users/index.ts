import { manifests as userGroupManifests } from './user-groups/manifests';
import { manifests as userManifests } from './users/manifests';
import { manifests as userSectionManifests } from './user-section/manifests';
import { manifests as currentUserManifests } from './current-user/manifests';

import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extensions-api';
import { ManifestTypes } from '@umbraco-cms/backoffice/extensions-registry';

export const manifests = [...userGroupManifests, ...userManifests, ...userSectionManifests, ...currentUserManifests];

const registerExtensions = (manifests: Array<ManifestTypes>) => {
	manifests.forEach((manifest) => umbExtensionsRegistry.register(manifest));
};

registerExtensions(manifests);
