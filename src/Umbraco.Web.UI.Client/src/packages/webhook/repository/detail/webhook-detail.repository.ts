import type { UmbWebhookDetailModel } from '../../types.js';
import { UmbWebhookServerDataSource } from './webhook-detail.server.data-source.js';
import { UMB_WEBHOOK_DETAIL_STORE_CONTEXT } from './webhook-detail.store.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbWebhookDetailRepository extends UmbDetailRepositoryBase<UmbWebhookDetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbWebhookServerDataSource, UMB_WEBHOOK_DETAIL_STORE_CONTEXT);
	}

	async create(model: UmbWebhookDetailModel) {
		return super.create(model, null);
	}
}

export default UmbWebhookDetailRepository;
