import type { UmbWebhookDetailModel } from '../../types.js';
import { UmbWebhookDetailServerDataSource } from './webhook-detail.server.data-source.js';
import { UMB_WEBHOOK_DETAIL_STORE_CONTEXT } from './webhook-detail.store.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbWebhookDetailRepository extends UmbDetailRepositoryBase<UmbWebhookDetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbWebhookDetailServerDataSource, UMB_WEBHOOK_DETAIL_STORE_CONTEXT);
	}

	override async create(model: UmbWebhookDetailModel) {
		return super.create(model, null);
	}

	async requestEvents(): Promise<{ data: { items: string[]; total: number }; error: any }> {
		//TODO Use service when available

		const items = ['Content Deleted', 'Content Published', 'Content Unpublished', 'Media Deleted', 'Media Saved'];

		const result = {
			data: { items, total: items.length },
			error: null,
		};

		return new Promise((resolve) => {
			setTimeout(() => resolve(result), 10);
		});
	}
}

export default UmbWebhookDetailRepository;
