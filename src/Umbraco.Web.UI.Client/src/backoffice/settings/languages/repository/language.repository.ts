import { UmbLanguageServerDataSource } from './sources/language.server.data';
import { UmbLanguageStore, UMB_LANGUAGE_STORE_CONTEXT_TOKEN } from './language.store';
import { UmbLanguageItemServerDataSource } from './sources/language-item.server.data';
import { UMB_LANGUAGE_ITEM_STORE_CONTEXT_TOKEN, UmbLanguageItemStore } from './language-item.store';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';
import { LanguageItemResponseModel, LanguageResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbItemRepository } from '@umbraco-cms/backoffice/repository';

export class UmbLanguageRepository implements UmbItemRepository<LanguageItemResponseModel> {
	#init: Promise<unknown>;

	#host: UmbControllerHostElement;

	#dataSource: UmbLanguageServerDataSource;
	#itemDataSource: UmbLanguageItemServerDataSource;
	#languageStore?: UmbLanguageStore;
	#languageItemStore?: UmbLanguageItemStore;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;

		this.#dataSource = new UmbLanguageServerDataSource(this.#host);
		this.#itemDataSource = new UmbLanguageItemServerDataSource(this.#host);

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this.#notificationContext = instance;
			}).asPromise(),

			new UmbContextConsumerController(this.#host, UMB_LANGUAGE_STORE_CONTEXT_TOKEN, (instance) => {
				this.#languageStore = instance;
			}).asPromise(),

			new UmbContextConsumerController(this.#host, UMB_LANGUAGE_ITEM_STORE_CONTEXT_TOKEN, (instance) => {
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

		return this.#dataSource.get(isoCode);
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

		const { error } = await this.#dataSource.insert(language);

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
