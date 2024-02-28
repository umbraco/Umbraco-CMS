import { manifests as workspaceModals } from './workspace-modal/manifests.js';
import { manifest as workspaceAliasCondition } from './workspace-alias.condition.js';
import { manifest as workspaceEntityTypeCondition } from './workspace-entity-type.condition.js';
import { manifests as workspaceActionMenuItemManifests } from './components/workspace-action/manifests.js';

export const manifests = [
	...workspaceModals,
	workspaceAliasCondition,
	workspaceEntityTypeCondition,
	...workspaceActionMenuItemManifests,
];
