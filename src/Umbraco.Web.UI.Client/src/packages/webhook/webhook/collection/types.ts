import type { UmbWebhookDetailModel } from '../types.js';
import type { UmbCollectionItemModel } from '@umbraco-cms/backoffice/collection';

export interface UmbWebhookCollectionItemModel extends UmbWebhookDetailModel, UmbCollectionItemModel {}

export interface UmbWebhookCollectionFilterModel {
	skip?: number;
	take?: number;
}
