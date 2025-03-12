import { manifests as propertyPickerModalManifests } from './property-picker-modal/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [...propertyPickerModalManifests];
