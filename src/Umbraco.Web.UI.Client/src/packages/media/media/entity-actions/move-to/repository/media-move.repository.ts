import { UmbMoveMediaServerDataSource } from './media-move.server.data-source.js';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbMoveToRepository, UmbMoveToRequestArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbMoveMediaRepository extends UmbRepositoryBase implements UmbMoveToRepository {
	#moveSource = new UmbMoveMediaServerDataSource(this);

	async requestMoveTo(args: UmbMoveToRequestArgs) {
		const { error } = await this.#moveSource.moveTo(args);

		if (!error) {
			const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
			const notification = { data: { message: `Moved` } };
			notificationContext.peek('positive', notification);
		}

		return { error };
	}
}
