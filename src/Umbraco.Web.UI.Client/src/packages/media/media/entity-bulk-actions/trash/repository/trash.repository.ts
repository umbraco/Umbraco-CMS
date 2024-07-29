import { UmbMediaRecycleBinServerDataSource } from '../../../recycle-bin/repository/media-recycle-bin.server.data-source.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbBulkTrashRepository, UmbBulkTrashRequestArgs } from '@umbraco-cms/backoffice/entity-bulk-action';
import type { UmbRepositoryErrorResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbBulkTrashMediaRepository extends UmbRepositoryBase implements UmbBulkTrashRepository {
	#notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;
	#recycleBinSource = new UmbMediaRecycleBinServerDataSource(this);

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (notificationContext) => {
			this.#notificationContext = notificationContext;
		});
	}

	async requestBulkTrash(args: UmbBulkTrashRequestArgs): Promise<UmbRepositoryErrorResponse> {
		let count = 0;

		for (const unique of args.uniques) {
			const { error } = await this.#recycleBinSource.trash({ unique });

			if (error) {
				const notification = { data: { message: error.message } };
				this.#notificationContext?.peek('danger', notification);
			} else {
				count++;
			}
		}

		if (count > 0) {
			const notification = { data: { message: `Trashed ${count} media ${count === 1 ? 'item' : 'items'}` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return {};
	}
}

export { UmbBulkTrashMediaRepository as api };
