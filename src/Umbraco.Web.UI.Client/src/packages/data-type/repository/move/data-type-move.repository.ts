import { UmbDataTypeMoveServerDataSource } from './data-type-move.server.data-source.js';
import type { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbMoveToDataSource, UmbMoveToRepository } from '@umbraco-cms/backoffice/repository';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbMoveDataTypeRepository extends UmbRepositoryBase implements UmbMoveToRepository {
	#init: Promise<unknown>;
	#moveSource: UmbMoveToDataSource;
	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#moveSource = new UmbDataTypeMoveServerDataSource(this);

		this.#init = Promise.all([
			this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
				this.#notificationContext = instance;
			}).asPromise(),
		]);
	}

	async requestMove(unique: string, targetUnique: string | null) {
		await this.#init;
		const { error } = await this.#moveSource.move(unique, targetUnique);

		if (!error) {
			const notification = { data: { message: `Data type moved` } };
			this.#notificationContext!.peek('positive', notification);
		}

		return { error };
	}
}
