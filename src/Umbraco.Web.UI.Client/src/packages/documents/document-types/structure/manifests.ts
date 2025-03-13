import { manifests as propertyTypePickerModalManifests } from './property-type-picker-modal/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [...propertyTypePickerModalManifests];
