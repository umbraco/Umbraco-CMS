import { manifests as workspaceAliasCondition } from './workspace-alias/manifests.js';
import { manifests as workspaceEntityIsNewCondition } from './workspace-entity-is-new/manifests.js';
import { manifests as workspaceEntityTypeCondition } from './workspace-entity-type/manifests.js';
import { manifest as workspaceIsLoadedCondition } from './workspace-is-loaded/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...workspaceAliasCondition,
	...workspaceEntityIsNewCondition,
	...workspaceEntityTypeCondition,
	workspaceIsLoadedCondition,
];
