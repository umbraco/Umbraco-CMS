import { manifests as translationSectionManifests } from './section.manifest';
import { manifests as dictionaryTreeManifests } from './dictionary/tree/dictionary-tree.manifest';
import { manifests as dictionaryWorkspaceManifests } from './dictionary/workspace/dictionary-workspace.manifest';
import { ManifestTypes, umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';

const registerExtensions = (manifests: Array<ManifestTypes>) => {
	manifests.forEach((manifest) => {
		if (umbExtensionsRegistry.isRegistered(manifest.alias)) return;
		umbExtensionsRegistry.register(manifest);
	});
};

registerExtensions([...translationSectionManifests, ...dictionaryTreeManifests, ...dictionaryWorkspaceManifests]);
