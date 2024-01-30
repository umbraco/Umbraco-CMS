import { type UmbDictionaryTreeStore, UMB_DICTIONARY_TREE_STORE_CONTEXT } from '../tree/index.js';
import { UmbDictionaryDetailServerDataSource } from './sources/dictionary-detail.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbTemporaryFileRepository } from '@umbraco-cms/backoffice/temporary-file';

export class UmbDictionaryRepository extends UmbBaseController implements UmbApi {
	#init!: Promise<unknown>;

	#treeStore?: UmbDictionaryTreeStore;

	#detailSource: UmbDictionaryDetailServerDataSource = new UmbDictionaryDetailServerDataSource(this);

	#temporaryFileRepository: UmbTemporaryFileRepository = new UmbTemporaryFileRepository(this);

	constructor(host: UmbControllerHost) {
		super(host);

		this.#init = Promise.all([
			this.consumeContext(UMB_DICTIONARY_TREE_STORE_CONTEXT, (instance) => {
				this.#treeStore = instance;
			}),
		]);
	}

	async itemsLegacy(ids: Array<string>) {
		await this.#init;
		return this.#treeStore!.items(ids);
	}

	async list(skip = 0, take = 1000) {
		await this.#init;
		return this.#detailSource.list(skip, take);
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
