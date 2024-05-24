import { manifest as reloadTreeItemChildrenKind } from './reload-tree-item-children.action.kind.js';
import type { ManifestTypes, UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [reloadTreeItemChildrenKind];
