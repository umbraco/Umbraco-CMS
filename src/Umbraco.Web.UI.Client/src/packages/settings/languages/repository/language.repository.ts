import { UmbLanguageServerDataSource } from './sources/language.server.data.js';
import { UmbLanguageStore, UMB_LANGUAGE_STORE_CONTEXT_TOKEN } from './language.store.js';
import { UmbLanguageItemServerDataSource } from './sources/language-item.server.data.js';
import { UMB_LANGUAGE_ITEM_STORE_CONTEXT_TOKEN, UmbLanguageItemStore } from './language-item.store.js';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';
import { LanguageItemResponseModel, LanguageResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbItemRepository } from '@umbraco-cms/backoffice/repository';

export class UmbLanguageRepository extends UmbBaseController implements UmbItemRepository<LanguageItemResponseModel> {
	#init: Promise<unknown>;

	#dataSource: UmbLanguageServerDataSource;
	#itemDataSource: UmbLanguageItemServerDataSource;
	#languageStore?: UmbLanguageStore;
	#languageItemStore?: UmbLanguageItemStore;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#dataSource = new UmbLanguageServerDataSource(this);
		this.#itemDataSource = new UmbLanguageItemServerDataSource(this);

		this.#init = Promise.all([
			this.consumeContext(UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this.#notificationContext = instance;
			}).asPromise(),

			this.consumeContext(UMB_LANGUAGE_STORE_CONTEXT_TOKEN, (instance) => {
				this.#languageStore = instance;
			}).asPromise(),

			this.consumeContext(UMB_LANGUAGE_ITEM_STORE_CONTEXT_TOKEN, (instance) => {
				this.#languageItemStore = instance;
			}).asPromise(),
		]);
	}

	// TODO: maybe this should be renamed to something more generic?
	async requestByIsoCode(isoCode: string) {
		await this.#init;

		if (!isoCode) {
			throw new Error('Iso code is missing');
		}

		return this.#dataSource.read(isoCode);
	}

	// TODO: maybe this should be renamed to something more generic.
	// Revisit when collection are in place
	async requestLanguages({ skip, take } = { skip: 0, take: 1000 }) {
		await this.#init;

		const { data, error } = await this.#dataSource.getCollection({ skip, take });

		if (data) {
			// TODO: allow to append an array of items to the store
			data.items.forEach((x) => this.#languageStore?.append(x));
		}

		return { data, error, asObservable: () => this.#languageStore!.data };
	}

	async requestItems(isoCodes: Array<string>) {
		await this.#init;
		const { data, error } = await this.#itemDataSource.getItems(isoCodes);

		if (data) {
			this.#languageItemStore?.appendItems(data);
		}

		return { data, error, asObservable: () => this.#languageItemStore!.items(isoCodes) };
	}

	async items(isoCodes: Array<string>) {
		await this.#init;
		return this.#languageItemStore!.items(isoCodes);
	}

	/**
	 * Creates a new Language scaffold
	 * @param
	 * @return {*}
	 * @memberof UmbLanguageRepository
	 */
	async createScaffold() {
		return this.#dataSource.createScaffold();
	}

	async create(language: LanguageResponseModel) {
		await this.#init;

		const { error } = await this.#dataSource.create(language);

		if (!error) {
			this.#languageStore?.append(language);
			const notification = { data: { message: `Language created` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	/**
	 * Saves a language
	 * @param {LanguageModel} language
	 * @return {*}
	 * @memberof UmbLanguageRepository
	 */
	async save(language: LanguageResponseModel) {
		if (!language.isoCode) throw new Error('Language iso code is missing');

		await this.#init;

		const { error } = await this.#dataSource.update(language.isoCode, language);

		if (!error) {
			const notification = { data: { message: `Language saved` } };
			this.#notificationContext?.peek('positive', notification);
			this.#languageStore?.append(language);
		}

		return { error };
	}

	/**
	 * Deletes a language
	 * @param {string} isoCode
	 * @return {*}
	 * @memberof UmbLanguageRepository
	 */
	async delete(isoCode: string) {
		await this.#init;

		if (!isoCode) {
			throw new Error('Iso code is missing');
		}

		const { error } = await this.#dataSource.delete(isoCode);

		if (!error) {
			this.#languageStore?.remove([isoCode]);
			const notification = { data: { message: `Language deleted` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}
}
