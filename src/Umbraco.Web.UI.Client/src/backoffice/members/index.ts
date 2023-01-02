import { manifests as memberGroupManifests } from './member-groups/manifests';
import { manifests as memberTypesManifests } from './member-types/manifests';
import { manifests as membersManifests } from './members/manifests';
import { manifests as memberSectionManifests } from './section.manifests';
import { ManifestTypes, umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';

const registerExtensions = (manifests: Array<ManifestTypes> | Array<ManifestTypes>) => {
	manifests.forEach((manifest) => {
		if (umbExtensionsRegistry.isRegistered(manifest.alias)) return;
		umbExtensionsRegistry.register(manifest);
	});
};

registerExtensions([...mediaSectionManifests]);
