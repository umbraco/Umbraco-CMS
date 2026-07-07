import { UMB_DOCUMENT_CONFIGURATION_REPOSITORY_ALIAS } from '../../../configuration/constants.js';
import { UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS } from '../../../repository/index.js';
import { UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS, UMB_DOCUMENT_REFERENCE_REPOSITORY_ALIAS } from '../../../constants.js';
import { UMB_DOCUMENT_PUBLISHING_REPOSITORY_ALIAS } from '../../repository/constants.js';
import type { MetaEntityActionContentUnpublishKind } from '@umbraco-cms/backoffice/content';

export const UMB_DOCUMENT_UNPUBLISH_META: MetaEntityActionContentUnpublishKind = {
	icon: 'icon-globe',
	label: '#actions_unpublish',
	additionalOptions: true,
	detailRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
	publishingRepositoryAlias: UMB_DOCUMENT_PUBLISHING_REPOSITORY_ALIAS,
	itemRepositoryAlias: UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS,
	referenceRepositoryAlias: UMB_DOCUMENT_REFERENCE_REPOSITORY_ALIAS,
	configurationRepositoryAlias: UMB_DOCUMENT_CONFIGURATION_REPOSITORY_ALIAS,
	unpublishedNotificationMessage: '#speechBubbles_editContentUnpublishedHeader',
};
