import type { UmbRelationTypeDetailModel } from '../../types.js';
import { UmbRelationTypeDetailServerDataSource } from './relation-type-detail.server.data-source.js';
import { UMB_RELATION_TYPE_DETAIL_STORE_CONTEXT } from './relation-type-detail.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbReadDetailRepository } from '@umbraco-cms/backoffice/repository';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbDetailStore } from '@umbraco-cms/backoffice/store';

// TODO: create read detail repository mixin
export class UmbRelationTypeDetailRepository
	extends UmbRepositoryBase
	implements UmbReadDetailRepository<UmbRelationTypeDetailModel>
{
	#init: Promise<unknown>;
	#detailStore?: UmbDetailStore<UmbRelationTypeDetailModel>;
	#detailSource = new UmbRelationTypeDetailServerDataSource(this);

	constructor(host: UmbControllerHost) {
		super(host);

		this.#init = Promise.all([
			this.consumeContext(UMB_RELATION_TYPE_DETAIL_STORE_CONTEXT, (instance) => {
				this.#detailStore = instance;
			}).asPromise(),
		]);
	}

	/**
	 * Requests the detail for the given unique
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbDetailRepositoryBase
	 */
	async requestByUnique(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		await this.#init;

		const { data, error } = await this.#detailSource.read(unique);

		if (data) {
			this.#detailStore!.append(data);
		}

		return { data, error, asObservable: () => this.#detailStore!.byUnique(unique) };
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
}

export default UmbRelationTypeDetailRepository;
