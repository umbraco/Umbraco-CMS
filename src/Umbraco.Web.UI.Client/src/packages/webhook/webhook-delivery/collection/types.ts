import type { UmbCollectionItemModel } from '@umbraco-cms/backoffice/collection';

export interface UmbWebhookDeliveryCollectionItemModel extends UmbCollectionItemModel {
	date: string;
	url: string;
	eventAlias: string;
	retryCount: number;
	statusCode: string;
}

export interface UmbWebhookDeliveryCollectionFilterModel {
	webhook: { unique: string };
	skip?: number;
	take?: number;
}
