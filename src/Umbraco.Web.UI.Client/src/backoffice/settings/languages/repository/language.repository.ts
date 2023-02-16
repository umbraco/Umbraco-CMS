import { UmbLanguageServerDataSource } from './sources/language.server.data';
import { UmbLanguageStore, UMB_LANGUAGE_STORE_CONTEXT_TOKEN } from './language.store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { UmbNotificationService, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/notification';
import { LanguageModel, ProblemDetailsModel } from '@umbraco-cms/backend-api';

export class UmbLanguageRepository {
	#init!: Promise<unknown>;

	#host: UmbControllerHostInterface;

	#detailDataSource: UmbLanguageServerDataSource;
	#languageStore?: UmbLanguageStore;

	#notificationService?: UmbNotificationService;

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;

		// TODO: figure out how spin up get the correct data source
		this.#detailDataSource = new UmbLanguageServerDataSource(this.#host);

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN, (instance) => {
				this.#notificationService = instance;
			}),

			new UmbContextConsumerController(this.#host, UMB_LANGUAGE_STORE_CONTEXT_TOKEN, (instance) => {
				this.#languageStore = instance;
			}),
		]);
	}

	/**
	 * Creates a new Language scaffold
	 * @param
	 * @return {*}
	 * @memberof UmbLanguageServerDataSource
	 */
	async createDetailsScaffold() {
		return this.#detailDataSource.createScaffold();
	}

	async requestByIsoCode(isoCode: string) {
		await this.#init;

		if (!isoCode) {
			const error: ProblemDetailsModel = { title: 'Iso code is missing' };
			return { error };
		}

		return this.#detailDataSource.get(isoCode);
	}

	// TODO: maybe this should be renamed to something more generic.
	// Revisit when collection are in place
	async requestLanguages({ skip, take } = { skip: 0, take: 1000 }) {
		await this.#init;

		const { data, error } = await this.#detailDataSource.getCollection({ skip, take });

		if (data) {
			// TODO: allow to append an array of items to the store
			data.items.forEach((x) => this.#languageStore?.append(x));
		}

		return { data, error, asObservable: () => this.#languageStore!.data };
	}

	async save(language: LanguageModel) {
		await this.#init;

		const { data, error } = await this.#detailDataSource.update(language);

		if (data) {
			const notification = { data: { message: `Language saved` } };
			this.#notificationService?.peek('positive', notification);
			this.#languageStore?.append(data);
		}

		return { data, error };
	}

	async create(language: LanguageModel) {
		await this.#init;

		const { data, error } = await this.#detailDataSource.update(language);

		if (data) {
			this.#languageStore?.append(data);
			const notification = { data: { message: `Language created` } };
			this.#notificationService?.peek('positive', notification);
		}

		return { data, error };
	}

	async requestItems(isoCode: Array<string>) {
		// HACK: filter client side until we have a proper server side endpoint
		const { data, error } = await this.requestLanguages();

		let items = undefined;

		if (data) {
			items = data.items = data.items.filter((x) => isoCode.includes(x.isoCode!));
		}

		return { data: items, error };
	}

	async delete(key: string) {
		await this.#init;

		if (!key) {
			const error: ProblemDetailsModel = { title: 'Language key is missing' };
			return { error };
		}

		const { error } = await this.#detailDataSource.delete(key);

		if (!error) {
			const notification = { data: { message: `Language deleted` } };
			this.#notificationService?.peek('positive', notification);
		}

		this.#languageStore?.remove([key]);

		return { error };
	}
}
