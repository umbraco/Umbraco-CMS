import { UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS } from '../../../repository/index.js';
import { UMB_DOCUMENT_PUBLISHING_REPOSITORY_ALIAS } from '../../repository/constants.js';
import type { MetaEntityActionContentPublishKind } from '@umbraco-cms/backoffice/content';

export const UmbDocumentPublishManifestEntityActionMeta: MetaEntityActionContentPublishKind = {
	icon: 'icon-globe',
	label: '#actions_publish',
	additionalOptions: true,
	detailRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
	publishingRepositoryAlias: UMB_DOCUMENT_PUBLISHING_REPOSITORY_ALIAS,
};
