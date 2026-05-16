import type { UmbWebhookEventCollectionFilterModel, UmbWebhookEventCollectionItemModel } from '../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';

export type UmbWebhookEventCollectionDataSource = UmbCollectionDataSource<
	UmbWebhookEventCollectionItemModel,
	UmbWebhookEventCollectionFilterModel
>;
