import type { UmbCollectionItemModel } from '@umbraco-cms/backoffice/collection';
import type { UmbWebhookEventEntityType } from '../entity.js';

export interface UmbWebhookEventCollectionItemModel extends UmbCollectionItemModel {
	entityType: UmbWebhookEventEntityType;
	unique: string;
	name: string;
	eventType: string;
	alias: string;
}

export interface UmbWebhookEventCollectionFilterModel {
	skip?: number;
	take?: number;
}
