import type { UmbRenameServerFileDataSource, UmbRenameServerFileDataSourceConstructor } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export abstract class UmbRenameServerFileRepositoryBase<
	DetailModelType extends { unique: string },
> extends UmbRepositoryBase {
	#renameSource: UmbRenameServerFileDataSource<DetailModelType>;

	constructor(host: UmbControllerHost, detailSource: UmbRenameServerFileDataSourceConstructor<DetailModelType>) {
		super(host);
		this.#renameSource = new detailSource(host);
	}

	/**
	 * Rename
	 * @param {string} unique
	 * @param {string} name
	 * @return {*}
	 * @memberof UmbRenameServerFileRepositoryBase
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
