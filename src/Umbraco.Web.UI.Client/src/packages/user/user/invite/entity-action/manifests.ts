import { manifests as inviteManifests } from './invite/manifests.js';
import { manifests as resendInviteManifests } from './resend-invite/manifests.js';

import type { ManifestTypes, UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbExtensionManifestKind> = [
	...inviteManifests,
	...resendInviteManifests,
];
