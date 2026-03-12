import type { UmbDocumentDetailModel } from '../../types.js';
import type { UmbPublishedPendingChangesManagerProcessArgs } from '@umbraco-cms/backoffice/content';

export type UmbDocumentPublishedPendingChangesManagerProcessArgs =
	UmbPublishedPendingChangesManagerProcessArgs<UmbDocumentDetailModel>;

export type { UmbPublishedVariantWithPendingChanges } from '@umbraco-cms/backoffice/content';
