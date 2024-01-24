import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_NOTIFICATION_CONTEXT, UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { UmbRenameDataSource, UmbRenameDataSourceConstructor } from '@umbraco-cms/backoffice/entity-action';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbDetailStore } from '@umbraco-cms/backoffice/store';

export abstract class UmbRenameRepositoryBase<DetailModelType extends { unique: string }> extends UmbRepositoryBase {
	#detailStore?: UmbDetailStore<DetailModelType>;
	#renameSource: UmbRenameDataSource<DetailModelType>;
	#notificationContext?: UmbNotificationContext;
	#init: Promise<unknown>;

	constructor(
		host: UmbControllerHost,
		detailSource: UmbRenameDataSourceConstructor<DetailModelType>,
		detailStoreContextAlias: string | UmbContextToken<any, any>,
	) {
		super(host);
		this.#renameSource = new detailSource(host);

		this.#init = Promise.all([
			this.consumeContext(detailStoreContextAlias, (instance) => {
				this.#detailStore = instance;
			}).asPromise(),

			this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
				this.#notificationContext = instance;
			}).asPromise(),
		]);
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
		await this.#init;

		const { data, error } = await this.#renameSource.rename(unique, name);

		if (data) {
			this.#detailStore?.removeItem(unique);
			this.#detailStore?.append(data);

			// TODO: how do we handle generic notifications? Is this the correct place to do it?
			const notification = { data: { message: `Renamed` } };
			this.#notificationContext!.peek('positive', notification);
		}

		return { data, error };
	}
}
