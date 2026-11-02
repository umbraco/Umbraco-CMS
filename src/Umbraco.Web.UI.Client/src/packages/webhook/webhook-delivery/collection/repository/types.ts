import type { UmbWebhookDeliveryCollectionFilterModel, UmbWebhookDeliveryCollectionItemModel } from '../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';

export type UmbWebhookDeliveryCollectionDataSource = UmbCollectionDataSource<
	UmbWebhookDeliveryCollectionItemModel,
	UmbWebhookDeliveryCollectionFilterModel
>;
