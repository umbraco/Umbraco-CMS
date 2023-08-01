import { manifests as workspaceModals } from './workspace-modal/manifests.js';
import { manifest as workspaceCondition } from './workspace-alias.condition.js';

export const manifests = [...workspaceModals, workspaceCondition];
