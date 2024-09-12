import { manifests as userAllowChangePasswordActionManifests } from './allow-change-password/manifests.js';
import { manifests as userAllowDeleteActionManifests } from './allow-delete/manifests.js';
import { manifests as userAllowDisableActionManifests } from './allow-disable/manifests.js';
import { manifests as userAllowEnableActionManifests } from './allow-enable/manifests.js';
import { manifests as userAllowExternalLoginActionManifests } from './allow-external-login/manifests.js';
import { manifests as userAllowMfaActionManifests } from './allow-mfa/manifests.js';
import { manifests as userAllowUnlockActionManifests } from './allow-unlock/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	...userAllowChangePasswordActionManifests,
	...userAllowDeleteActionManifests,
	...userAllowDisableActionManifests,
	...userAllowEnableActionManifests,
	...userAllowExternalLoginActionManifests,
	...userAllowMfaActionManifests,
	...userAllowUnlockActionManifests,
];
