import { manifest as reloadTreeItemChildrenKind } from './reload-tree-item-children.action.kind.js';
import type { ManifestTypes, UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbExtensionManifestKind> = [reloadTreeItemChildrenKind];
