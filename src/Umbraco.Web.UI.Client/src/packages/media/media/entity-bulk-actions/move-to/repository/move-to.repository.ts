import { UmbMoveMediaServerDataSource } from '../../../entity-actions/move-to/repository/media-move.server.data-source.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbBulkMoveToRepository, UmbBulkMoveToRequestArgs } from '@umbraco-cms/backoffice/entity-bulk-action';
import type { UmbRepositoryErrorResponse } from '@umbraco-cms/backoffice/repository';

export class UmbBulkMoveToMediaRepository extends UmbRepositoryBase implements UmbBulkMoveToRepository {
	#moveSource = new UmbMoveMediaServerDataSource(this);

	async requestBulkMoveTo(args: UmbBulkMoveToRequestArgs): Promise<UmbRepositoryErrorResponse> {
		const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
		let count = 0;

		const destination = args.destination;
		for (const unique of args.uniques) {
			const { error } = await this.#moveSource.moveTo({ unique, destination });

			if (error) {
				const notification = { data: { message: error.message } };
				notificationContext?.peek('danger', notification);
			} else {
				count++;
			}
		}

		if (count > 0) {
			const notification = { data: { message: `Moved ${count} media ${count === 1 ? 'item' : 'items'}` } };
			notificationContext?.peek('positive', notification);
		}

		return {};
	}
}

export { UmbBulkMoveToMediaRepository as api };
