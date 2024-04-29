import { manifest as editableKindManifest } from './editable/editable-workspace.kind.js';
import { manifest as routableKindManifest } from './routable/routable-workspace.kind.js';
import type { ManifestTypes, UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [routableKindManifest, editableKindManifest];
