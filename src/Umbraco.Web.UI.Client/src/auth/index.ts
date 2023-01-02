// TODO: user manifests should first be registered when the user logs in
import { manifests as userGroupManifests } from './users/user-groups/manifests';
import { manifests as userManifests } from './users/users/manifests';
import { manifests as userSectionManifests } from './users/user-section/manifests';

import { ManifestTypes, umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';

const registerExtensions = (manifests: Array<ManifestTypes>) => {
	manifests.forEach((manifest) => {
		if (umbExtensionsRegistry.isRegistered(manifest.alias)) return;
		umbExtensionsRegistry.register(manifest);
	});
};

registerExtensions([...userSectionManifests, ...userGroupManifests, ...userManifests]);
