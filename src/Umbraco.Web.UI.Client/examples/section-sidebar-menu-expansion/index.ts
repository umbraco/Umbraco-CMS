import { manifests as playgroundDashboardManifests } from './playground-dashboard/manifests.js';
import { manifests as collapseMenuItemManifests } from './collapse-menu-item-entity-action/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...playgroundDashboardManifests, ...collapseMenuItemManifests];
