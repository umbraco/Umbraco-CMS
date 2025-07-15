import { manifests as clipboardManifests } from './clipboard/manifests.js';
import { manifests as propertValueClonerManifests } from './property-value-cloner/manifests.js';
import { manifests as propertyEditorManifests } from './property-editors/manifests.js';
import { manifests as validationManifests } from './validation/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...clipboardManifests,
	...propertValueClonerManifests,
	...propertyEditorManifests,
	...validationManifests,
	...workspaceManifests,
];
