import { manifests as userAllowChangePasswordActionManifests } from './allow-change-password/manifests.js';
import { manifests as userAllowDeleteActionManifests } from './allow-delete/manifests.js';
import { manifests as userAllowDisableActionManifests } from './allow-disable/manifests.js';
import { manifests as userAllowEnableActionManifests } from './allow-enable/manifests.js';
import { manifests as userAllowExternalLoginActionManifests } from './allow-external-login/manifests.js';
import { manifests as userAllowMfaActionManifests } from './allow-mfa/manifests.js';
import { manifests as userAllowUnlockActionManifests } from './allow-unlock/manifests.js';
import { manifests as userIsDefaultKindManifests } from './is-default-kind/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...userAllowChangePasswordActionManifests,
	...userAllowDeleteActionManifests,
	...userAllowDisableActionManifests,
	...userAllowEnableActionManifests,
	...userAllowExternalLoginActionManifests,
	...userAllowMfaActionManifests,
	...userAllowUnlockActionManifests,
	...userIsDefaultKindManifests,
];
