import { manifests as workspaceAliasCondition } from './workspace-content-type-alias/manifests.js';
import { manifests as WorkspaceContentTypeUnique } from './workspace-content-type-unique/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...workspaceAliasCondition, ...WorkspaceContentTypeUnique];
