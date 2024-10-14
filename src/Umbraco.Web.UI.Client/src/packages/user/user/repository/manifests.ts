import { manifests as avatarManifests } from './avatar/manifests.js';
import { manifests as changePasswordManifests } from './change-password/manifests.js';
import { manifests as configManifests } from './config/manifests.js';
import { manifests as detailManifests } from './detail/manifests.js';
import { manifests as disableManifests } from './disable/manifests.js';
import { manifests as enableManifests } from './enable/manifests.js';
import { manifests as itemManifests } from './item/manifests.js';
import { manifests as unlockManifests } from './unlock/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...detailManifests,
	...itemManifests,
	...avatarManifests,
	...changePasswordManifests,
	...configManifests,
	...disableManifests,
	...enableManifests,
	...unlockManifests,
];
