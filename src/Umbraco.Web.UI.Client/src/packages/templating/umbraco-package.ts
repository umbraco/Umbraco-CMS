import { manifests as menuManifests } from './menu/manifests.js';
import { manifests as templateManifests } from './templates/manifests.js';
import { manifests as stylesheetManifests } from './stylesheets/manifests.js';
import { manifests as partialManifests } from './partial-views/manifests.js';
import { manifests as scriptManifest } from './scripts/manifests.js';
import { manifests as modalManifests } from './modals/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import * as entryPointModule from './entry-point.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...menuManifests,
	...templateManifests,
	...stylesheetManifests,
	...partialManifests,
	...modalManifests,
	...scriptManifest,
];

export const name = 'Umbraco.Core.Templating';
export const extensions = [
	{
		name: 'Template Management Bundle',
		alias: 'Umb.Bundle.TemplateManagement',
		type: 'bundle',
		js: {
			manifests,
		},
	},
	{
		name: 'Template Management Backoffice Entry Point',
		alias: 'Umb.BackofficeEntryPoint.TemplateManagement',
		type: 'backofficeEntryPoint',
		js: entryPointModule,
	},
];
