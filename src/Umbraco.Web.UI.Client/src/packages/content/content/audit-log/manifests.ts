import { manifests as auditLogActionManifests } from './audit-log-action/manifests.js';
import { manifests as infoAppManifests } from './info-app/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...auditLogActionManifests,
	...infoAppManifests,
];
