import { manifests as renameModalManifests } from './modal/manifests.js';
import { manifest as renameKindManifest } from './rename-server-file.action.kind.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...renameModalManifests,
	renameKindManifest,
];
