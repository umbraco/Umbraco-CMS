import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as menuManifests } from './menu/manifests.js';
import { manifests as propertyEditorManifests } from './property-editors/manifests.js';
import * as entryPointModule from './entry-point.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...repositoryManifests,
	...workspaceManifests,
	...collectionManifests,
	...menuManifests,
	...propertyEditorManifests,
	{
		name: 'Relation Type Backoffice Entry Point',
		alias: 'Umb.EntryPoint.RelationType',
		type: 'backofficeEntryPoint',
		js: entryPointModule,
	},
];
