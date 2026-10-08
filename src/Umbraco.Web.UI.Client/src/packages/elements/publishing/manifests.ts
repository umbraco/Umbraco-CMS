import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as workspaceContextManifests } from './workspace-context/manifests.js';
import { manifests as publishManifests } from './publish/manifests.js';
import { manifests as unpublishManifests } from './unpublish/manifests.js';
import { manifests as schedulePublishManifests } from './schedule-publish/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...repositoryManifests,
	...workspaceContextManifests,
	...publishManifests,
	...unpublishManifests,
	...schedulePublishManifests,
];
