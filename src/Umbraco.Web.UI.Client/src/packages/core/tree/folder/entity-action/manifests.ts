import { manifest as createKindManifest } from './create-folder/create-folder.action.kind.js';
import { manifest as deleteKindManifest } from './delete-folder/delete-folder.action.kind.js';
import { manifest as updateKindManifest } from './update-folder/update-folder.action.kind.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	createKindManifest,
	deleteKindManifest,
	updateKindManifest,
];
