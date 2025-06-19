import { manifest as workspaceAliasCondition } from './workspace-alias.condition.js';
import { manifest as workspaceEntityTypeCondition } from './workspace-entity-type.condition.js';
import { manifest as workspaceEntityIsNewCondition } from './workspace-entity-is-new.condition.js';

export const manifests: Array<UmbExtensionManifest> = [
	workspaceEntityIsNewCondition,
	workspaceAliasCondition,
	workspaceEntityTypeCondition,
];
