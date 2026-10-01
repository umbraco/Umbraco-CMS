import { UmbElementPublishAwarenessApi } from './element-publish-awareness.api.js';
import { UMB_ELEMENT_ITEM_REPOSITORY_ALIAS } from '../../constants.js';
import { UMB_ELEMENT_ENTITY_TYPE } from '../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityPublishAwareness',
		alias: 'Umb.EntityPublishAwareness.Element',
		name: 'Element Entity Publish Awareness',
		api: UmbElementPublishAwarenessApi,
		forEntityTypes: [UMB_ELEMENT_ENTITY_TYPE],
		meta: { itemRepositoryAlias: UMB_ELEMENT_ITEM_REPOSITORY_ALIAS },
	},
];
