import { UmbDuplicateDataTypeServerDataSource } from './data-type-duplicate.server.data-source.js';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbDuplicateToRepository, UmbDuplicateToRequestArgs } from '@umbraco-cms/backoffice/tree';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbDuplicateDataTypeRepository extends UmbRepositoryBase implements UmbDuplicateToRepository {
	#duplicateSource = new UmbDuplicateDataTypeServerDataSource(this);

	async requestDuplicateTo(args: UmbDuplicateToRequestArgs) {
		const { error } = await this.#duplicateSource.duplicateTo(args);

		if (!error) {
			const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
			const notification = { data: { message: `Duplicated` } };
			notificationContext.peek('positive', notification);
		}

		return { error };
	}
}

export { UmbDuplicateDataTypeRepository as api };
