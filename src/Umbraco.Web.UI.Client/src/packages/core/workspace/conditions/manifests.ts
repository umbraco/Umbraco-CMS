import { manifest as workspaceAliasCondition } from './workspace-alias.condition.js';
import { manifest as workspaceEntityTypeCondition } from './workspace-entity-type.condition.js';
import { manifest as workspaceHasCollectionCondition } from './workspace-has-collection.condition.js';
import { manifest as workspaceContentTypeAliasCondition } from './workspace-content-type-alias.condition.js';

export const manifests: Array<UmbExtensionManifest> = [
	workspaceAliasCondition,
	workspaceEntityTypeCondition,
	workspaceHasCollectionCondition,
	workspaceContentTypeAliasCondition,
];
