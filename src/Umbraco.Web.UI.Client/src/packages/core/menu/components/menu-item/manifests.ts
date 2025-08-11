import { manifests as actionManifests } from './action/manifests.js';
import { manifests as linkManifests } from './link/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [...actionManifests, ...linkManifests];
