import { manifest as multiLanguageConditionManifest } from './conditions/multiple-app-languages/multiple-app-languages.condition.js';
import { manifest as userPermissionConditionManifest } from './conditions/language-user-permission/manifests.js';
import { manifests as appLanguageSelect } from './app-language-select/manifests.js';
import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as entityActions } from './entity-actions/manifests.js';
import { manifests as globalContextManifests } from './global-contexts/manifests.js';
import { manifests as itemManifests } from './item/manifests.js';
import { manifests as menuManifests } from './menu/manifests.js';
import { manifests as modalManifests } from './modals/manifests.js';
import { manifests as propertyEditorManifests } from './property-editor/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import * as entryPointModule from './entry-point.js';

export const manifests: Array<UmbExtensionManifest> = [
	...appLanguageSelect,
	...collectionManifests,
	...entityActions,
	...globalContextManifests,
	...itemManifests,
	...menuManifests,
	...modalManifests,
	...propertyEditorManifests,
	...repositoryManifests,
	...workspaceManifests,
	multiLanguageConditionManifest,
	userPermissionConditionManifest,
	{
		name: 'Language Backoffice Entry Point',
		alias: 'Umb.EntryPoint.Language',
		type: 'backofficeEntryPoint',
		js: entryPointModule,
	},
];

export const name = 'Umbraco.Core.Language';
export const extensions = [
	{
		name: 'Language Bundle',
		alias: 'Umb.Bundle.Language',
		type: 'bundle',
		js: {
			manifests,
		},
	},
];
