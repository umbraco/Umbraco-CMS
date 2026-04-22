import { UMB_LANGUAGE_ROOT_ENTITY_TYPE } from '../../entity.js';
import { manifests as defaultManifests } from './default/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'create',
		alias: 'Umb.EntityAction.Language.Create',
		name: 'Create Language Entity Action',
		weight: 1200,
		forEntityTypes: [UMB_LANGUAGE_ROOT_ENTITY_TYPE],
	},
	...defaultManifests,
];
