import type { UmbWebhookDetailModel } from '../../types.js';
import type { UmbWebhookCollectionFilterModel } from '../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';

export type UmbWebhookCollectionDataSource = UmbCollectionDataSource<
	UmbWebhookDetailModel,
	UmbWebhookCollectionFilterModel
>;
