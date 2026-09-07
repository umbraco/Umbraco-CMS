import { UmbRepositoryBase } from '../repository-base.js';
import type {
	UmbRepositoryErrorResponse,
	UmbRepositoryResponse,
	UmbRepositoryResponseWithAsObservable,
} from '../types.js';
import type { UmbDetailDataSource, UmbDetailDataSourceConstructor } from './detail-data-source.interface.js';
import type { UmbDetailRepository } from './detail-repository.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbDetailStore } from '@umbraco-cms/backoffice/store';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbDeepPartialObject } from '@umbraco-cms/backoffice/utils';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

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

	constructor(
		host: UmbControllerHost,
		detailSource: UmbDetailDataSourceConstructor<DetailModelType>,
		detailStoreContextAlias: string | UmbContextToken<any, any>,
	) {
		super(host);

		if (!detailSource) throw new Error('Detail source is missing');
		if (!detailStoreContextAlias) throw new Error('Detail store context alias is missing');

		this.detailDataSource = new detailSource(host) as UmbDetailDataSourceType;

		// TODO: ideally no preventTimeouts here.. [NL]
		this.#init = this.consumeContext(detailStoreContextAlias, (instance) => {
			this.#detailStore = instance;
		})
			.asPromise({ preventTimeout: true })
			// Ignore the error, we can assume that the flow was stopped (asPromise failed), but it does not mean that the consumption was not successful.
			.catch(() => undefined);
	}

	/**
	 * Creates a scaffold
	 * @param {UmbDeepPartialObject<DetailModelType>} [preset] - Partial data to seed the scaffold with
	 * @returns {Promise<UmbRepositoryResponse<DetailModelType>>} The scaffolded detail data
	 * @memberof UmbDetailRepositoryBase
	 */
	async createScaffold(
		preset?: UmbDeepPartialObject<DetailModelType>,
	): Promise<UmbRepositoryResponse<DetailModelType>> {
		return this.detailDataSource.createScaffold(preset);
	}

	/**
	 * Requests the detail for the given unique
	 * @param {string} unique - The unique identifier of the detail
	 * @returns {Promise<UmbRepositoryResponseWithAsObservable<DetailModelType | undefined>>} The requested detail data
	 * @memberof UmbDetailRepositoryBase
	 */
	async requestByUnique(unique: string): Promise<UmbRepositoryResponseWithAsObservable<DetailModelType | undefined>> {
		if (!unique) throw new Error('Unique is missing');
		await this.#init;

		const { data, error } = await this.detailDataSource.read(unique);

		if (data) {
			this.#detailStore?.append(data);
		}

		return {
			data,
			error,
			asObservable: () => this.#detailStore?.byUnique(unique),
		};
	}

	/**
	 * Returns a promise with an observable of the detail for the given unique
	 * @param {DetailModelType} model - The data to create
	 * @param {string | null} [parentUnique] - The unique of the parent, if any
	 * @returns {Promise<UmbRepositoryResponse<DetailModelType>>} The created detail data
	 * @memberof UmbDetailRepositoryBase
	 */
	async create(model: DetailModelType, parentUnique: string | null): Promise<UmbRepositoryResponse<DetailModelType>> {
		if (!model) throw new Error('Data is missing');
		await this.#init;

		const { data: createdData, error } = await this.detailDataSource.create(model, parentUnique);

		if (createdData) {
			this.#detailStore?.append(createdData);
		}

		return { data: createdData, error };
	}

	/**
	 * Saves the given data
	 * @param {DetailModelType} model - The data to save
	 * @returns {Promise<UmbRepositoryResponse<DetailModelType>>} The saved detail data
	 * @memberof UmbDetailRepositoryBase
	 */
	async save(model: DetailModelType) {
		if (!model) throw new Error('Data is missing');
		if (!model.unique) throw new Error('Unique is missing');
		await this.#init;

		const { data: updatedData, error } = await this.detailDataSource.update(model);

		if (updatedData) {
			this.#detailStore?.updateItem(model.unique, updatedData);
		}

		return { data: updatedData, error };
	}

	/**
	 * Deletes the detail for the given unique
	 * @param {string} unique - The unique identifier of the detail to delete
	 * @returns {Promise<UmbRepositoryErrorResponse>} The result of the delete request
	 * @memberof UmbDetailRepositoryBase
	 */
	async delete(unique: string): Promise<UmbRepositoryErrorResponse> {
		if (!unique) throw new Error('Unique is missing');
		await this.#init;

		const { error } = await this.detailDataSource.delete(unique);

		if (!error) {
			this.#detailStore?.removeItem(unique);
		}

		return { error };
	}

	/**
	 * Returns a promise with an observable of the detail for the given unique
	 * @param {string} unique - The unique identifier of the detail
	 * @returns {Promise<Observable<DetailModelType | undefined>>} An observable of the detail data
	 * @memberof UmbDetailRepositoryBase
	 */
	async byUnique(unique: string): Promise<Observable<DetailModelType | undefined>> {
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
