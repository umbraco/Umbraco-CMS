import { manifests as customCollectionManifests } from './custom-collection/manifests.js';
import { manifests as customTreeManifests } from './custom-tree/manifests.js';
import { manifests as documentManifests } from './document/manifests.js';
import { manifests as languageManifests } from './language/manifests.js';
import { manifests as mediaManifests } from './media/manifests.js';
import { manifests as userManifests } from './user/manifests.js';
import { manifests as webhookManifests } from './webhook/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...customCollectionManifests,
	...customTreeManifests,
	...documentManifests,
	...languageManifests,
	...mediaManifests,
	...userManifests,
	...webhookManifests,
];
