import { manifest as allowMfaManifest } from './allow-mfa/current-user-allow-mfa-action.condition.manifest.js';
import { manifest as groupManifest } from './group-id/group-id.condition.manifest.js';
import { manifest as isAdminManifest } from './is-admin/is-admin.condition.manifest.js';

export const manifests = [allowMfaManifest, groupManifest, isAdminManifest];
