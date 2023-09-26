import { manifests as userGroupManifests } from './user-groups/manifests.js';
import { manifests as userManifests } from './users/manifests.js';
import { manifests as userSectionManifests } from './user-section/manifests.js';
import { manifests as currentUserManifests } from './current-user/manifests.js';

// We need to load any components that are not loaded by the user management bundle to register them in the browser.
import './user-groups/components/index.js';
import './users/components/index.js';

export const manifests = [...userGroupManifests, ...userManifests, ...userSectionManifests, ...currentUserManifests];
