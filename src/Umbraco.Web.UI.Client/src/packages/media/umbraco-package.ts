import { manifests as mediaManifests } from './media/manifests.js';
import { manifests as mediaSectionManifests } from './media-section/manifests.js';
import { manifests as mediaTypesManifests } from './media-types/manifests.js';
import { manifests as imagingManifests } from './imaging/manifests.js';
import { manifests as dropzoneManifests } from './dropzone/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import * as entryPointModule from './entry-point.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...mediaSectionManifests,
	...mediaManifests,
	...mediaTypesManifests,
	...imagingManifests,
	...dropzoneManifests,
];

export const name = 'Umbraco.Core.MediaManagement';
export const extensions = [
	{
		name: 'Media Management Bundle',
		alias: 'Umb.Bundle.MediaManagement',
		type: 'bundle',
		js: {
			manifests,
		},
	},
	{
		name: 'Media Management Entry Point',
		alias: 'Umb.EntryPoint.MediaManagement',
		type: 'backofficeEntryPoint',
		js: entryPointModule,
	},
];
