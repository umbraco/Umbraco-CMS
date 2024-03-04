import { manifests as componentManifests } from './components/manifests.js';
import { manifests as workspaceModals } from './workspace-modal/manifests.js';
import { manifest as workspaceAliasCondition } from './workspace-alias.condition.js';
import { manifest as workspaceEntityTypeCondition } from './workspace-entity-type.condition.js';

export const manifests = [
	...componentManifests,
	...workspaceModals,
	workspaceAliasCondition,
	workspaceEntityTypeCondition,
];
