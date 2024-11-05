import { UmbRepositoryBase } from '../repository-base.js';
import type { UmbRepositoryResponse, UmbRepositoryResponseWithAsObservable } from '../types.js';
import type { UmbDetailDataSource, UmbDetailDataSourceConstructor } from './detail-data-source.interface.js';
import type { UmbDetailRepository } from './detail-repository.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbDetailStore } from '@umbraco-cms/backoffice/store';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export abstract class UmbDetailRepositoryBase<
		DetailModelType extends UmbEntityModel,
		UmbDetailDataSourceType extends UmbDetailDataSource<DetailModelType> = UmbDetailDataSource<DetailModelType>,
	>
	extends UmbRepositoryBase
	implements UmbDetailRepository<DetailModelType>, UmbApi
{
	#init: Promise<unknown>;

	#detailStore?: UmbDetailStore<DetailModelType>;
	protected detailDataSource: UmbDetailDataSourceType;
	#notificationContext?: UmbNotificationContext;

	constructor(
		host: UmbControllerHost,
		detailSource: UmbDetailDataSourceConstructor<DetailModelType>,
		detailStoreContextAlias: string | UmbContextToken<any, any>,
	) {
		super(host);

		if (!detailSource) throw new Error('Detail source is missing');
		if (!detailStoreContextAlias) throw new Error('Detail store context alias is missing');

		this.detailDataSource = new detailSource(host) as UmbDetailDataSourceType;

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
	 * Creates a scaffold
	 * @param {Partial<DetailModelType>} [preset]
	 * @returns {*}
	 * @memberof UmbDetailRepositoryBase
	 */
	async createScaffold(preset?: Partial<DetailModelType>): Promise<UmbRepositoryResponse<DetailModelType>> {
		return this.detailDataSource.createScaffold(preset);
	}

	/**
	 * Requests the detail for the given unique
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbDetailRepositoryBase
	 */
	async requestByUnique(unique: string): Promise<UmbRepositoryResponseWithAsObservable<DetailModelType>> {
		if (!unique) throw new Error('Unique is missing');
		await this.#init;

		const { data, error } = await this.detailDataSource.read(unique);

		if (data) {
			this.#detailStore!.append(data);
		}

		return {
			data,
			error,
			asObservable: () => this.#detailStore!.byUnique(unique),
		};
	}

	/**
	 * Returns a promise with an observable of the detail for the given unique
	 * @param {DetailModelType} model
	 * @param {string | null} [parentUnique]
	 * @returns {*}
	 * @memberof UmbDetailRepositoryBase
	 */
	async create(model: DetailModelType, parentUnique: string | null): Promise<UmbRepositoryResponse<DetailModelType>> {
		if (!model) throw new Error('Data is missing');
		await this.#init;

		const { data: createdData, error } = await this.detailDataSource.create(model, parentUnique);

		if (createdData) {
			this.#detailStore?.append(createdData);

			// TODO: how do we handle generic notifications? Is this the correct place to do it?
			const notification = { data: { message: `Created` } };
			this.#notificationContext!.peek('positive', notification);
		}

		return { data: createdData, error };
	}

	/**
	 * Saves the given data
	 * @param {DetailModelType} model
	 * @returns {*}
	 * @memberof UmbDetailRepositoryBase
	 */
	async save(model: DetailModelType) {
		if (!model) throw new Error('Data is missing');
		if (!model.unique) throw new Error('Unique is missing');
		await this.#init;

		const { data: updatedData, error } = await this.detailDataSource.update(model);

		if (updatedData) {
			this.#detailStore!.updateItem(model.unique, updatedData);

			// TODO: how do we handle generic notifications? Is this the correct place to do it?
			const notification = { data: { message: `Saved` } };
			this.#notificationContext!.peek('positive', notification);
		}

		return { data: updatedData, error };
	}

	/**
	 * Deletes the detail for the given unique
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbDetailRepositoryBase
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		await this.#init;

		const { error } = await this.detailDataSource.delete(unique);

		if (!error) {
			this.#detailStore!.removeItem(unique);

			// TODO: how do we handle generic notifications? Is this the correct place to do it?
			const notification = { data: { message: `Deleted` } };
			this.#notificationContext!.peek('positive', notification);
		}

		return { error };
	}

	/**
	 * Returns a promise with an observable of the detail for the given unique
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbDetailRepositoryBase
	 */
	async byUnique(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		await this.#init;
		return this.#detailStore!.byUnique(unique);
	}

	override destroy(): void {
		this.#detailStore = undefined;
		(this.detailDataSource as unknown) = undefined;
		super.destroy();
	}
}
