import { UmbDataTypeDetailModel } from '../../types.js';
import { UmbDataTypeServerDataSource } from './data-type-detail.server.data-source.js';
import { UMB_DATA_TYPE_DETAIL_STORE_CONTEXT, UmbDataTypeDetailStore } from './data-type-detail.store.js';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailDataSource, UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { UMB_NOTIFICATION_CONTEXT_TOKEN, UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
export class UmbDataTypeDetailRepository extends UmbRepositoryBase {
	#init: Promise<unknown>;

	#detailStore?: UmbDataTypeDetailStore;
	#detailSource: UmbDetailDataSource<UmbDataTypeDetailModel>;
	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#detailSource = new UmbDataTypeServerDataSource(host);

		this.#init = Promise.all([
			this.consumeContext(UMB_DATA_TYPE_DETAIL_STORE_CONTEXT, (instance) => {
				this.#detailStore = instance;
			}).asPromise(),

			this.consumeContext(UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this.#notificationContext = instance;
			}).asPromise(),
		]);
	}

	async createScaffold(parentUnique: string | null) {
		if (parentUnique === undefined) throw new Error('Parent unique is missing');
		return this.#detailSource.createScaffold(parentUnique);
	}

	async requestByUnique(unique: string) {
		if (!unique) throw new Error('Key is missing');
		await this.#init;

		const { data, error } = await this.#detailSource.read(unique);

		if (data) {
			this.#detailStore!.append(data);
		}

		return { data, error, asObservable: () => this.#detailStore!.byUnique(unique) };
	}

	async byUnique(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		await this.#init;
		return this.#detailStore!.byUnique(unique);
	}

	async byPropertyEditorUiAlias(propertyEditorUiAlias: string) {
		if (!propertyEditorUiAlias) throw new Error('propertyEditorUiAlias is missing');
		await this.#init;
		return this.#detailStore!.withPropertyEditorUiAlias(propertyEditorUiAlias);
	}

	async create(dataType: UmbDataTypeDetailModel) {
		if (!dataType) throw new Error('Data Type is missing');
		if (!dataType.unique) throw new Error('Data Type unique is missing');
		await this.#init;

		const { error } = await this.#detailSource.create(dataType);

		if (!error) {
			this.#detailStore?.append(dataType);

			const notification = { data: { message: `Data Type created` } };
			this.#notificationContext!.peek('positive', notification);
		}

		return { error };
	}

	async save(dataType: UmbDataTypeDetailModel) {
		if (!dataType) throw new Error('Data Type is missing');
		if (!dataType.unique) throw new Error('Data Type unique is missing');
		await this.#init;

		const { data, error } = await this.#detailSource.update(dataType);

		if (data) {
			this.#detailStore!.updateItem(data.unique, data);

			const notification = { data: { message: `Data Type saved` } };
			this.#notificationContext!.peek('positive', notification);
		}

		return { data, error };
	}

	async delete(unique: string) {
		if (!unique) throw new Error('Data Type unique is missing');
		await this.#init;

		const { error } = await this.#detailSource.delete(unique);

		if (!error) {
			this.#detailStore!.removeItem(unique);

			const notification = { data: { message: `Data Type deleted` } };
			this.#notificationContext!.peek('positive', notification);
		}

		return { error };
	}
}
