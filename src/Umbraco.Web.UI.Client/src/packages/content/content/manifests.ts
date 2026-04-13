import { manifests as auditLogManifests } from './audit-log/manifests.js';
import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as conditionManifests } from './conditions/manifests.js';
import { manifests as rollbackManifests } from './rollback/manifests.js';
import { manifests as contentTreeManifests } from './tree/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';

export const manifests = [
	...auditLogManifests,
	...collectionManifests,
	...conditionManifests,
	...rollbackManifests,
	...contentTreeManifests,
	...workspaceManifests,
];
