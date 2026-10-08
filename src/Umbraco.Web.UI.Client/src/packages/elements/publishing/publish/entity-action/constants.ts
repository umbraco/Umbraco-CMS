import { UMB_ELEMENT_DETAIL_REPOSITORY_ALIAS } from '../../../repository/detail/constants.js';
import { UMB_ELEMENT_PUBLISHING_REPOSITORY_ALIAS } from '../../repository/constants.js';
import type { MetaEntityActionContentPublishKind } from '@umbraco-cms/backoffice/content';

export const UmbElementPublishManifestEntityActionMeta: MetaEntityActionContentPublishKind = {
	icon: 'icon-globe',
	label: '#actions_publish',
	additionalOptions: true,
	detailRepositoryAlias: UMB_ELEMENT_DETAIL_REPOSITORY_ALIAS,
	publishingRepositoryAlias: UMB_ELEMENT_PUBLISHING_REPOSITORY_ALIAS,
	publishedNotificationMessage: '#speechBubbles_editElementPublishedHeader',
	variantPublishedNotificationMessage: '#speechBubbles_editVariantElementPublishedText',
};
