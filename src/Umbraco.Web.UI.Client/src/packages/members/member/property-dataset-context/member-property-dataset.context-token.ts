import { UMB_MEMBER_ENTITY_TYPE } from '../entity.js';
import type { UmbMemberPropertyDatasetContext } from './member-property-dataset.context.js';
import type { UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const IsMemberVariantContext = (
	context: UmbPropertyDatasetContext,
): context is UmbMemberPropertyDatasetContext => context.getEntityType() === UMB_MEMBER_ENTITY_TYPE;

export const UMB_MEMBER_VARIANT_CONTEXT = new UmbContextToken<
	UmbPropertyDatasetContext,
	UmbMemberPropertyDatasetContext
>('UmbVariantContext', undefined, IsMemberVariantContext);
