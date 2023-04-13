import { UmbDictionaryStore, UMB_DICTIONARY_STORE_CONTEXT_TOKEN } from './dictionary.store';
import { UmbDictionaryDetailServerDataSource } from './sources/dictionary.detail.server.data';
import { UmbDictionaryTreeStore, UMB_DICTIONARY_TREE_STORE_CONTEXT_TOKEN } from './dictionary.tree.store';
import { DictionaryTreeServerDataSource } from './sources/dictionary.tree.server.data';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbTreeDataSource, UmbDetailRepository, UmbTreeRepository } from '@umbraco-cms/backoffice/repository';
import {
	CreateDictionaryItemRequestModel,
	DictionaryOverviewResponseModel,
	ImportDictionaryRequestModel,
	ProblemDetailsModel,
	UpdateDictionaryItemRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';

export class UmbDictionaryRepository
	implements
		UmbTreeRepository,
		UmbDetailRepository<
			CreateDictionaryItemRequestModel,
			UpdateDictionaryItemRequestModel,
			DictionaryOverviewResponseModel
		>
{
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

	async requestTreeItemsOf(parentId: string | null) {
		await this.#init;

		if (!parentId) {
			const error: ProblemDetailsModel = { title: 'Parent id is missing' };
			return { data: undefined, error };
		}

		const { data, error } = await this.#treeSource.getChildrenOf(parentId);

		if (data) {
			this.#treeStore?.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#treeStore!.childrenOf(parentId) };
	}

	async requestTreeItems(ids: Array<string>) {
		await this.#init;

		if (!ids) {
			const error: ProblemDetailsModel = { title: 'Keys are missing' };
			return { data: undefined, error };
		}

		const { data, error } = await this.#treeSource.getItems(ids);

		return { data, error, asObservable: () => this.#treeStore!.items(ids) };
	}

	async rootTreeItems() {
		await this.#init;
		return this.#treeStore!.rootItems;
	}

	async treeItemsOf(parentId: string | null) {
		await this.#init;
		return this.#treeStore!.childrenOf(parentId);
	}

	async treeItems(ids: Array<string>) {
		await this.#init;
		return this.#treeStore!.items(ids);
	}

	// DETAILS

	async createScaffold(parentId: string | null, name?: string) {
		if (parentId === undefined) throw new Error('Parent id is missing');
		await this.#init;
		return this.#detailSource.createScaffold(parentId, name);
	}

	async requestById(id: string) {
		if (!id) throw new Error('Id is missing');
		await this.#init;

		const { data, error } = await this.#detailSource.get(id);

		if (data) {
			this.#detailStore?.append(data);
		}
		return { data, error };
	}

	async list(skip = 0, take = 1000) {
		await this.#init;
		return this.#detailSource.list(skip, take);
	}

	async delete(id: string) {
		await this.#init;
		return this.#detailSource.delete(id);
	}

	async save(id: string, updatedDictionary: UpdateDictionaryItemRequestModel) {
		if (!id) throw new Error('Id is missing');
		if (!updatedDictionary) throw new Error('Dictionary is missing');

		await this.#init;

		const { error } = await this.#detailSource.update(id, updatedDictionary);

		if (!error) {
			// TODO: we currently don't use the detail store for anything.
			// Consider to look up the data before fetching from the server
			// Consider notify a workspace if a dictionary is updated in the store while someone is editing it.
			// TODO: would be nice to align the stores on methods/methodNames.
			//this.#detailStore?.append(dictionary);
			this.#treeStore?.updateItem(id, { name: updatedDictionary.name });

			const notification = { data: { message: `Dictionary '${updatedDictionary.name}' saved` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	async create(detail: CreateDictionaryItemRequestModel) {
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

	async export(id: string, includeChildren = false) {
		await this.#init;

		if (!id) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		return this.#detailSource.export(id, includeChildren);
	}

	async import(temporaryFileId: string, parentId?: string) {
		await this.#init;

		if (!temporaryFileId) {
			const error: ProblemDetailsModel = { title: 'File is missing' };
			return { error };
		}

		return this.#detailSource.import(temporaryFileId, parentId);
	}

	async upload(formData: ImportDictionaryRequestModel) {
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
