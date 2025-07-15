import { manifest as reloadTreeItemChildrenKind } from './reload-tree-item-children.action.kind.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [reloadTreeItemChildrenKind];
