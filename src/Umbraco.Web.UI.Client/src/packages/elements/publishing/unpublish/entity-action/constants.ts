import { UMB_ELEMENT_CONFIGURATION_REPOSITORY_ALIAS } from '../../../configuration/constants.js';
import { UMB_ELEMENT_DETAIL_REPOSITORY_ALIAS } from '../../../repository/detail/constants.js';
import { UMB_ELEMENT_ITEM_REPOSITORY_ALIAS, UMB_ELEMENT_REFERENCE_REPOSITORY_ALIAS } from '../../../constants.js';
import { UMB_ELEMENT_PUBLISHING_REPOSITORY_ALIAS } from '../../repository/constants.js';
import type { MetaEntityActionContentUnpublishKind } from '@umbraco-cms/backoffice/content';

export const UmbElementUnpublishManifestEntityActionMeta: MetaEntityActionContentUnpublishKind = {
	icon: 'icon-globe',
	label: '#actions_unpublish',
	additionalOptions: true,
	detailRepositoryAlias: UMB_ELEMENT_DETAIL_REPOSITORY_ALIAS,
	publishingRepositoryAlias: UMB_ELEMENT_PUBLISHING_REPOSITORY_ALIAS,
	itemRepositoryAlias: UMB_ELEMENT_ITEM_REPOSITORY_ALIAS,
	referenceRepositoryAlias: UMB_ELEMENT_REFERENCE_REPOSITORY_ALIAS,
	configurationRepositoryAlias: UMB_ELEMENT_CONFIGURATION_REPOSITORY_ALIAS,
	unpublishedNotificationMessage: '#speechBubbles_editElementUnpublishedHeader',
};
