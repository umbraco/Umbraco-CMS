import { manifests as componentManifests } from './components/manifests.js';
import { manifests as workspaceKinds } from './kinds/manifests.js';
import { manifests as workspaceModals } from './modals/manifests.js';
import { manifests as workspaceConditions } from './conditions/manifests.js';

export const manifests = [...workspaceConditions, ...workspaceKinds, ...componentManifests, ...workspaceModals];
