import { manifests as publishManifest } from './publish/manifests.js';
import { manifests as publishWithDescendantsManifest } from './publish-with-descendants/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as schedulePublishManifests } from './schedule-publish/manifests.js';
import { manifests as unpublishManifests } from './unpublish/manifests.js';
import { manifests as workspaceContextManifests } from './workspace-context/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...publishManifest,
	...publishWithDescendantsManifest,
	...repositoryManifests,
	...schedulePublishManifests,
	...unpublishManifests,
	...workspaceContextManifests,
];
