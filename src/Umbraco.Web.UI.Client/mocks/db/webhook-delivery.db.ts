import type { UmbMockWebhookDeliveryModel } from '../data/mock-data-set.types.js';
import { UmbMockDBBase } from './utils/mock-db-base.js';

interface WebhookDeliveryFilterOptions {
	webhookId: string;
	skip?: number;
	take?: number;
}

class UmbWebhookDeliveryMockDB extends UmbMockDBBase<UmbMockWebhookDeliveryModel> {
	constructor(data: Array<UmbMockWebhookDeliveryModel>) {
		super('webhookDelivery', data);
	}

	filter(options: WebhookDeliveryFilterOptions): { items: Array<UmbMockWebhookDeliveryModel>; total: number } {
		const skip = options.skip ?? 0;
		const take = options.take ?? 10;
		const filtered = this.getAll().filter((log) => log.webhookKey === options.webhookId);
		return { items: filtered.slice(skip, skip + take), total: filtered.length };
	}
}

export const umbWebhookDeliveryMockDb = new UmbWebhookDeliveryMockDB([]);
