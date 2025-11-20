import { manifests as sortChildrenOfContentManifests } from './sort-children-of-content/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [...sortChildrenOfContentManifests];
