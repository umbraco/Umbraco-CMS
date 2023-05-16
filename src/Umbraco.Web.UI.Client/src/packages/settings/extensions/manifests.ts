import { manifests as menuItemManifests } from './menu-item/manifests';
import { manifests as workspaceManifests } from './workspace/manifests';

export const manifests = [...menuItemManifests, ...workspaceManifests];
