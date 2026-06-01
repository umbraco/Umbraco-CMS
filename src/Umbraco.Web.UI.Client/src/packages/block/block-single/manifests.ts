import { manifests as clipboardManifests } from './clipboard/manifests.js';
import { manifests as propertyValueClonerManifests } from './property-value-cloner/manifests.js';
import { manifests as propertyEditorManifests } from './property-editors/manifests.js';
import { manifests as validationManifests } from './validation/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { manifests as valueSummaryManifests } from './property-editors/block-single-editor/value-summary/manifests.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...clipboardManifests,
	...propertyValueClonerManifests,
	...propertyEditorManifests,
	...validationManifests,
	...workspaceManifests,
	...valueSummaryManifests,
];
