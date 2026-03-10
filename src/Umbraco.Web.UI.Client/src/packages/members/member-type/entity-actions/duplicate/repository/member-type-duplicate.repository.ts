import { UmbDuplicateMemberTypeServerDataSource } from './member-type-duplicate.server.data-source.js';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbDuplicateToRepository, UmbDuplicateToRequestArgs } from '@umbraco-cms/backoffice/tree';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbDuplicateMemberTypeRepository extends UmbRepositoryBase implements UmbDuplicateToRepository {
	#duplicateSource = new UmbDuplicateMemberTypeServerDataSource(this);

	async requestDuplicateTo(args: UmbDuplicateToRequestArgs) {
		const { error } = await this.#duplicateSource.duplicateTo(args);

		if (!error) {
			const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
			if (!notificationContext) {
				throw new Error('Notification context not found');
			}
			const notification = { data: { message: `Duplicated` } };
			notificationContext.peek('positive', notification);
		}

		return { error };
	}
}

export { UmbDuplicateMemberTypeRepository as api };
