import { manifests as auditLogActionManifests } from './audit-log-action/manifests.js';
import { manifests as entityActionManifests } from './entity-action/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...auditLogActionManifests,
	...entityActionManifests,
	...repositoryManifests,
];
