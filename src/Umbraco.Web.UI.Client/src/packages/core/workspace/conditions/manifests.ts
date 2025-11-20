import { manifests as workspaceAliasCondition } from './workspace-alias/manifests.js';
import { manifests as workspaceEntityIsNewCondition } from './workspace-entity-is-new/manifests.js';
import { manifests as workspaceEntityTypeCondition } from './workspace-entity-type/manifests.js';
import { manifests as workspaceViewUnique } from './workspace-content-type-unique/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...workspaceAliasCondition,
	...workspaceEntityIsNewCondition,
	...workspaceEntityTypeCondition,
	...workspaceViewUnique,
];
