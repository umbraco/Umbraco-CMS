import { manifests as userGroupManifests } from './user-groups/manifests.js';
import { manifests as userManifests } from './users/manifests.js';
import { manifests as userSectionManifests } from './user-section/manifests.js';
import { manifests as currentUserManifests } from './current-user/manifests.js';

export const manifests = [...userGroupManifests, ...userManifests, ...userSectionManifests, ...currentUserManifests];
