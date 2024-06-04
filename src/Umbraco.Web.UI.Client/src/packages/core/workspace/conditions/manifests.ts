import { manifest as workspaceAliasCondition } from './workspace-alias.condition.js';
import { manifest as workspaceEntityTypeCondition } from './workspace-entity-type.condition.js';
import { manifest as workspaceHasCollectionCondition } from './workspace-has-collection.condition.js';
import { manifest as workspaceContentTypeAliasCondition } from './workspace-content-type-alias.condition.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	workspaceAliasCondition,
	workspaceEntityTypeCondition,
	workspaceHasCollectionCondition,
	workspaceContentTypeAliasCondition
];
