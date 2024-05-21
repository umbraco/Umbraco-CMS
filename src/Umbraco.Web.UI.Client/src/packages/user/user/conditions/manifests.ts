import { manifest as userAllowDisableActionManifest } from './user-allow-disable-action.condition.js';
import { manifest as userAllowEnableActionManifest } from './user-allow-enable-action.condition.js';
import { manifest as userAllowUnlockActionManifest } from './user-allow-unlock-action.condition.js';
import { manifest as userAllowExternalLoginActionManifest } from './user-allow-external-login-action.condition.js';
import { manifest as userAllowMfaActionManifest } from './user-allow-mfa-action.condition.js';
import { manifest as userAllowDeleteActionManifest } from './user-allow-delete-action.condition.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	userAllowDisableActionManifest,
	userAllowEnableActionManifest,
	userAllowUnlockActionManifest,
	userAllowExternalLoginActionManifest,
	userAllowMfaActionManifest,
	userAllowDeleteActionManifest,
];
