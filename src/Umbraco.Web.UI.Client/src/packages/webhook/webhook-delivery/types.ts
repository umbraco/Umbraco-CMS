import type { UmbWebhookDeliveryEntityType } from '../entity.js';

export interface UmbWebhookDeliveryDetailModel {
	entityType: UmbWebhookDeliveryEntityType;
	unique: string;
	date: string;
	url: string;
	eventAlias: string;
	retryCount: number;
	statusCode: string;
}
