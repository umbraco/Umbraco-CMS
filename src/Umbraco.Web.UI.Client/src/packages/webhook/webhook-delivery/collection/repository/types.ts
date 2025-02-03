import type { UmbWebhookDeliveryDetailModel } from '../../types.js';
import type { UmbWebhookDeliveryCollectionFilterModel } from '../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';

export type UmbWebhookDeliveryCollectionDataSource = UmbCollectionDataSource<
	UmbWebhookDeliveryDetailModel,
	UmbWebhookDeliveryCollectionFilterModel
>;
