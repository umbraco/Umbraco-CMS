import { UmbLanguageServerDataSource } from './sources/language.server.data';
import { UmbLanguageStore, UMB_LANGUAGE_STORE_CONTEXT_TOKEN } from './language.store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { UmbNotificationService, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/notification';
import { ProblemDetailsModel } from '@umbraco-cms/backend-api';

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

	async requestLanguages({ skip, take } = { skip: 0, take: 1000 }) {
		await this.#init;

		const { data, error } = await this.#detailDataSource.getCollection({ skip, take });

		if (data) {
			// TODO: allow to append an array of items to the store
			data.items.forEach((x) => this.#languageStore?.append(x));
		}

		return { data, error, asObservable: () => this.#languageStore!.data };
	}
}
