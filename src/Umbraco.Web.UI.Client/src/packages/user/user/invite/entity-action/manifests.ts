import { manifests as inviteManifests } from './invite/manifests.js';
import { manifests as resendInviteManifests } from './resend-invite/manifests.js';

import type { ManifestTypes, UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [
	...inviteManifests,
	...resendInviteManifests,
];
