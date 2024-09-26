// eslint-disable-next-line local-rules/no-relative-import-to-import-map-module
import { manifests as tiptapManifests } from './tiptap/manifests.js';
import type { ManifestTypes, UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbExtensionManifestKind> = [...tiptapManifests];
