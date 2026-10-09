import { manifests as auditLogManifests } from './audit-log/manifests.js';
import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as conditionManifests } from './conditions/manifests.js';
import { manifests as languageAccessManifests } from './language-access/manifests.js';
import { manifests as publishingManifests } from './publishing/manifests.js';
import { manifests as rollbackManifests } from './rollback/manifests.js';
import { manifests as contentTreeManifests } from './tree/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...auditLogManifests,
	...collectionManifests,
	...conditionManifests,
	...languageAccessManifests,
	...publishingManifests,
	...rollbackManifests,
	...contentTreeManifests,
	...workspaceManifests,
];
