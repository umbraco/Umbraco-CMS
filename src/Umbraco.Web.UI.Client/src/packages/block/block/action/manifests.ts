import { manifests as copyToClipboardManifests } from './common/copy-to-clipboard/manifests.js';
import { manifests as deleteManifests } from './common/delete/manifests.js';
import { manifests as editSettingsManifests } from './common/edit-settings/manifests.js';
import { manifests as defaultKindManifests } from './default/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...defaultKindManifests,
	...copyToClipboardManifests,
	...deleteManifests,
	...editSettingsManifests,
];
