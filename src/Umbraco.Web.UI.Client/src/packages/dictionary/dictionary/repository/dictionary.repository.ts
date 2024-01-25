import { type UmbDictionaryTreeStore, UMB_DICTIONARY_TREE_STORE_CONTEXT } from '../tree/index.js';
import type { UmbDictionaryStore} from './dictionary.store.js';
import { UMB_DICTIONARY_STORE_CONTEXT } from './dictionary.store.js';
import { UmbDictionaryDetailServerDataSource } from './sources/dictionary-detail.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import type {
	CreateDictionaryItemRequestModel,
	UpdateDictionaryItemRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import { type UmbNotificationContext, UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbTemporaryFileRepository } from '@umbraco-cms/backoffice/temporary-file';

export class UmbDictionaryRepository extends UmbBaseController implements UmbApi {
	#init!: Promise<unknown>;

	#treeStore?: UmbDictionaryTreeStore;

	#detailSource: UmbDictionaryDetailServerDataSource = new UmbDictionaryDetailServerDataSource(this);
	#detailStore?: UmbDictionaryStore;

	#temporaryFileRepository: UmbTemporaryFileRepository = new UmbTemporaryFileRepository(this);

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#init = Promise.all([
			this.consumeContext(UMB_DICTIONARY_STORE_CONTEXT, (instance) => {
				this.#detailStore = instance;
			}),

			this.consumeContext(UMB_DICTIONARY_TREE_STORE_CONTEXT, (instance) => {
				this.#treeStore = instance;
			}),

			this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
				this.#notificationContext = instance;
			}),
		]);
	}

	async itemsLegacy(ids: Array<string>) {
		await this.#init;
		return this.#treeStore!.items(ids);
	}

	// DETAILS

	// TODO: consider if we want to create a specific createScaffoldWithName, to loose the coupling to the model.
	async createScaffold(parentId: string | null, preset?: Partial<CreateDictionaryItemRequestModel>) {
		if (parentId === undefined) throw new Error('Parent id is missing');
		await this.#init;
		return this.#detailSource.createScaffold(parentId, preset);
	}

	async requestById(id: string) {
		if (!id) throw new Error('Id is missing');
		await this.#init;

		const { data, error } = await this.#detailSource.read(id);

		if (data) {
			this.#detailStore?.append(data);
		}

		return { data, error };
	}

	async byId(id: string) {
		if (!id) throw new Error('Key is missing');
		await this.#init;
		return this.#detailStore!.byId(id);
	}

	async list(skip = 0, take = 1000) {
		await this.#init;
		return this.#detailSource.list(skip, take);
	}

	async delete(id: string) {
		await this.#init;
		await this.#treeStore?.removeItem(id);
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
			throw new Error('Name is missing');
		}

		const { data, error } = await this.#detailSource.create(detail);

		if (!error) {
			const notification = { data: { message: `Dictionary '${detail.name}' created` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { data, error };
	}

	async export(id: string, includeChildren = false) {
		await this.#init;

		if (!id) {
			throw new Error('Id is missing');
		}

		return this.#detailSource.export(id, includeChildren);
	}

	async import(temporaryFileId: string, parentId?: string) {
		await this.#init;

		if (!temporaryFileId) {
			throw new Error('Temporary file id is missing');
		}

		return this.#detailSource.import(temporaryFileId, parentId);
	}

	async upload(UmbId: string, file: File) {
		await this.#init;
		if (!UmbId) throw new Error('UmbId is missing');
		if (!file) throw new Error('File is missing');

		return this.#temporaryFileRepository.upload(UmbId, file);
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
