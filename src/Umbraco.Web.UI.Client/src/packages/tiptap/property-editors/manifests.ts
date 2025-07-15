// eslint-disable-next-line local-rules/no-relative-import-to-import-map-module
import { manifests as tiptapManifests } from './tiptap/manifests.js';
import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifests: Array<ManifestPropertyEditorUi> = [...tiptapManifests];
