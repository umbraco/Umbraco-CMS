import type { UmbDocumentDetailModel } from '../../types.js';
import type { UmbContentPublishedPendingChangesManagerProcessArgs } from '@umbraco-cms/backoffice/content';

export type UmbDocumentPublishedPendingChangesManagerProcessArgs =
	UmbContentPublishedPendingChangesManagerProcessArgs<UmbDocumentDetailModel>;

export type { UmbPublishedVariantWithPendingChanges } from '@umbraco-cms/backoffice/content';
