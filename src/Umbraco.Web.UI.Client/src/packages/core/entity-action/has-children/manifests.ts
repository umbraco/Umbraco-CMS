import { manifest as conditionManifest } from './condition/entity-has-children.condition.manifest.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [conditionManifest];
