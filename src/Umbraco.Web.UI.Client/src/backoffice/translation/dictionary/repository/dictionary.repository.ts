import type { DictionaryDetails } from '../';
import { UmbDictionaryStore, UMB_DICTIONARY_STORE_CONTEXT_TOKEN } from './dictionary.store';
import { UmbDictionaryDetailServerDataSource } from './sources/dictionary.detail.server.data';
import { UmbDictionaryTreeStore, UMB_DICTIONARY_TREE_STORE_CONTEXT_TOKEN } from './dictionary.tree.store';
import { DictionaryTreeServerDataSource } from './sources/dictionary.tree.server.data';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbTreeDataSource, UmbDetailRepository, UmbTreeRepository } from '@umbraco-cms/backoffice/repository';
import { ProblemDetailsModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';

export class UmbDictionaryRepository implements UmbTreeRepository, UmbDetailRepository<DictionaryDetails> {
	#init!: Promise<unknown>;

	#host: UmbControllerHostElement;

	#treeSource: UmbTreeDataSource;
	#treeStore?: UmbDictionaryTreeStore;

	#detailSource: UmbDictionaryDetailServerDataSource;
	#detailStore?: UmbDictionaryStore;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;

		// TODO: figure out how spin up get the correct data source
		this.#treeSource = new DictionaryTreeServerDataSource(this.#host);
		this.#detailSource = new UmbDictionaryDetailServerDataSource(this.#host);

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_DICTIONARY_STORE_CONTEXT_TOKEN, (instance) => {
				this.#detailStore = instance;
			}),

			new UmbContextConsumerController(this.#host, UMB_DICTIONARY_TREE_STORE_CONTEXT_TOKEN, (instance) => {
				this.#treeStore = instance;
			}),

			new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this.#notificationContext = instance;
			}),
		]);
	}

	async requestRootTreeItems() {
		await this.#init;

		const { data, error } = await this.#treeSource.getRootItems();

		if (data) {
			this.#treeStore?.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#treeStore!.rootItems };
	}

	async requestTreeItemsOf(parentKey: string | null) {
		await this.#init;

		if (!parentKey) {
			const error: ProblemDetailsModel = { title: 'Parent key is missing' };
			return { data: undefined, error };
		}

		const { data, error } = await this.#treeSource.getChildrenOf(parentKey);

		if (data) {
			this.#treeStore?.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#treeStore!.childrenOf(parentKey) };
	}

	async requestTreeItems(keys: Array<string>) {
		await this.#init;

		if (!keys) {
			const error: ProblemDetailsModel = { title: 'Keys are missing' };
			return { data: undefined, error };
		}

		const { data, error } = await this.#treeSource.getItems(keys);

		return { data, error, asObservable: () => this.#treeStore!.items(keys) };
	}

	async rootTreeItems() {
		await this.#init;
		return this.#treeStore!.rootItems;
	}

	async treeItemsOf(parentKey: string | null) {
		await this.#init;
		return this.#treeStore!.childrenOf(parentKey);
	}

	async treeItems(keys: Array<string>) {
		await this.#init;
		return this.#treeStore!.items(keys);
	}

	// DETAILS

	async createScaffold(parentKey: string | null) {
		await this.#init;

		if (!parentKey) {
			const error: ProblemDetailsModel = { title: 'Parent key is missing' };
			return { data: undefined, error };
		}

		return this.#detailSource.createScaffold(parentKey);
	}

	async requestByKey(key: string) {
		await this.#init;

		// TODO: should we show a notification if the key is missing?
		// Investigate what is best for Acceptance testing, cause in that perspective a thrown error might be the best choice?
		if (!key) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}
		const { data, error } = await this.#detailSource.get(key);

		if (data) {
			this.#detailStore?.append(data);
		}
		return { data, error };
	}

	async list(skip = 0, take = 1000) {
		await this.#init;
		return this.#detailSource.list(skip, take);
	}

	async delete(key: string) {
		await this.#init;
		return this.#detailSource.delete(key);
	}

	async save(dictionary: DictionaryDetails) {
		await this.#init;

		// TODO: should we show a notification if the dictionary is missing?
		// Investigate what is best for Acceptance testing, cause in that perspective a thrown error might be the best choice?
		if (!dictionary || !dictionary.key) {
			const error: ProblemDetailsModel = { title: 'Dictionary is missing' };
			return { error };
		}

		const { error } = await this.#detailSource.update(dictionary);

		if (!error) {
			const notification = { data: { message: `Dictionary '${dictionary.name}' saved` } };
			this.#notificationContext?.peek('positive', notification);
		}

		// TODO: we currently don't use the detail store for anything.
		// Consider to look up the data before fetching from the server
		// Consider notify a workspace if a dictionary is updated in the store while someone is editing it.
		this.#detailStore?.append(dictionary);
		this.#treeStore?.updateItem(dictionary.key, { name: dictionary.name });
		// TODO: would be nice to align the stores on methods/methodNames.

		return { error };
	}

	async create(detail: DictionaryDetails) {
		await this.#init;

		if (!detail.name) {
			const error: ProblemDetailsModel = { title: 'Name is missing' };
			return { error };
		}

		const { data, error } = await this.#detailSource.insert(detail);

		if (!error) {
			const notification = { data: { message: `Dictionary '${detail.name}' created` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { data, error };
	}

	async export(key: string, includeChildren = false) {
		await this.#init;

		if (!key) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		return this.#detailSource.export(key, includeChildren);
	}

	async import(fileName: string, parentKey?: string) {
		await this.#init;

		if (!fileName) {
			const error: ProblemDetailsModel = { title: 'File is missing' };
			return { error };
		}

		return this.#detailSource.import(fileName, parentKey);
	}

	async upload(formData: FormData) {
		await this.#init;

		if (!formData) {
			const error: ProblemDetailsModel = { title: 'Form data is missing' };
			return { error };
		}

		return this.#detailSource.upload(formData);
	}

	// TODO => temporary only, until languages data source exists, or might be
	// ok to keep, as it reduces downstream dependencies
	async getLanguages() {
		await this.#init;

		const { data } = await this.#detailSource.getLanguages();

		// default first, then sorted by name
		// easier to unshift than conditionally sorting by bool and string
		const languages =
			data?.items.sort((a, b) => {
				a.name = a.name ?? '';
				b.name = b.name ?? '';
				return a.name > b.name ? 1 : b.name > a.name ? -1 : 0;
			}) ?? [];

		const defaultIndex = languages.findIndex((x) => x.isDefault);
		languages.unshift(...languages.splice(defaultIndex, 1));

		return languages;
	}

	async move() {
		alert('move me!');
	}
}
