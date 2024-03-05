import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbRenameDataSource, UmbRenameDataSourceConstructor } from '@umbraco-cms/backoffice/entity-action';
import type { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export abstract class UmbRenameRepositoryBase<DetailModelType extends { unique: string }> extends UmbRepositoryBase {
	#renameSource: UmbRenameDataSource<DetailModelType>;

	constructor(host: UmbControllerHost, detailSource: UmbRenameDataSourceConstructor<DetailModelType>) {
		super(host);
		this.#renameSource = new detailSource(host);
	}

	/**
	 * Rename
	 * @param {string} unique
	 * @param {string} name
	 * @return {*}
	 * @memberof UmbRenameRepositoryBase
	 */
	async rename(unique: string, name: string) {
		if (!unique) throw new Error('Unique is missing');
		if (!name) throw new Error('Name is missing');

		const { data, error } = await this.#renameSource.rename(unique, name);

		if (data) {
			const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
			const notification = { data: { message: `Renamed` } };
			notificationContext.peek('positive', notification);
		}

		return { data, error };
	}
}
