import type { UmbMockWebhookModel } from '../data/mock-data-set.types.js';
import { UmbEntityMockDbBase } from './utils/entity/entity-base.js';
import { UmbMockEntityItemManager } from './utils/entity/entity-item.manager.js';
import { UmbMockEntityDetailManager } from './utils/entity/entity-detail.manager.js';
import { UmbId } from '@umbraco-cms/backoffice/id';

class UmbWebhookMockDB extends UmbEntityMockDbBase<UmbMockWebhookModel> {
	item = new UmbMockEntityItemManager<UmbMockWebhookModel>(this, itemResponseMapper);
	detail = new UmbMockEntityDetailManager<UmbMockWebhookModel>(this, createDetailMockMapper, detailResponseMapper);

	constructor(data: Array<UmbMockWebhookModel>) {
		super('webhook', data);
	}
}

const createDetailMockMapper = (request: any): UmbMockWebhookModel => {
	return {
		id: request.id ? request.id : UmbId.new(),
		name: request.name ?? null,
		description: request.description ?? null,
		url: request.url,
		enabled: request.enabled ?? true,
		events: request.events ?? [],
		contentTypeKeys: request.contentTypeKeys ?? [],
		headers: request.headers ?? {},
	};
};

const detailResponseMapper = (item: UmbMockWebhookModel) => {
	return {
		id: item.id,
		name: item.name,
		description: item.description,
		url: item.url,
		enabled: item.enabled,
		events: item.events,
		contentTypeKeys: item.contentTypeKeys,
		headers: item.headers,
	};
};

const itemResponseMapper = (item: UmbMockWebhookModel) => {
	return {
		id: item.id,
		name: item.name ?? '',
		enabled: item.enabled,
		events: item.events.map((e) => e.eventName).join(', '),
		url: item.url,
		types: item.contentTypeKeys.join(', '),
	};
};

export const umbWebhookMockDb = new UmbWebhookMockDB([]);
