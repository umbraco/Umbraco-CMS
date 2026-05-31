import type { UmbMockWebhookEventModel } from '../data/mock-data-set.types.js';
import { UmbMockDBBase } from './utils/mock-db-base.js';

class UmbWebhookEventMockDB extends UmbMockDBBase<UmbMockWebhookEventModel> {
	constructor(data: Array<UmbMockWebhookEventModel>) {
		super('webhookEvent', data);
	}
}

export const umbWebhookEventMockDb = new UmbWebhookEventMockDB([]);
