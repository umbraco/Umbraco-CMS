import { UmbScriptTreeRepository } from '../tree/index.js';
import { UmbScriptServerDataSource } from './sources/script-detail.server.data.js';
import { UmbScriptFolderServerDataSource } from './sources/script-folder.server.data.js';
import { UmbDetailRepository, UmbFolderRepository } from '@umbraco-cms/backoffice/repository';
import {
	CreateFolderRequestModel,
	CreateScriptRequestModel,
	FolderResponseModel,
	ScriptResponseModel,
	UpdateScriptRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbId } from '@umbraco-cms/backoffice/id';

export class UmbScriptRepository
	extends UmbBaseController
	implements
		UmbDetailRepository<CreateScriptRequestModel, string, UpdateScriptRequestModel, ScriptResponseModel, string>,
		UmbFolderRepository,
		UmbApi
{
	#detailDataSource: UmbScriptServerDataSource;
	#folderDataSource: UmbScriptFolderServerDataSource;

	// TODO: temp solution until it is automated
	#treeRepository = new UmbScriptTreeRepository(this);

	constructor(host: UmbControllerHost) {
		super(host);

		this.#detailDataSource = new UmbScriptServerDataSource(this);
		this.#folderDataSource = new UmbScriptFolderServerDataSource(this);
	}

	//#region FOLDER
	createFolderScaffold(parentId: string | null) {
		const data: FolderResponseModel = {
			id: UmbId.new(),
			name: '',
			parentId,
		};
		return Promise.resolve({ data, error: undefined });
	}

	async createFolder(requestBody: CreateFolderRequestModel) {
		const req = {
			parentPath: requestBody.parentId,
			name: requestBody.name,
		};
		const promise = this.#folderDataSource.create(req);
		await promise;
		this.#treeRepository.requestTreeItemsOf(requestBody.parentId ? requestBody.parentId : null);
		return promise;
	}

	async requestFolder(unique: string) {
		return this.#folderDataSource.read(unique);
	}

	updateFolder(): any {
		throw new Error('Method not implemented.');
	}

	async deleteFolder(path: string) {
		const { data } = await this.requestFolder(path);
		const promise = this.#folderDataSource.delete(path);
		await promise;
		this.#treeRepository.requestTreeItemsOf(data?.parentPath ? data?.parentPath : null);
		return promise;
	}
	//#endregion

	//#region DETAILS
	async requestByKey(path: string) {
		if (!path) throw new Error('Path is missing');
		const { data, error } = await this.#detailDataSource.read(path);
		return { data, error };
	}

	requestById(): any {
		throw new Error('Method not implemented.');
	}

	byId(): any {
		throw new Error('Method not implemented.');
	}

	createScaffold(parentId: string | null, preset: string) {
		return this.#detailDataSource.createScaffold(parentId, preset);
	}

	async create(data: CreateScriptRequestModel) {
		const promise = this.#detailDataSource.create(data);
		await promise;
		this.#treeRepository.requestTreeItemsOf(data.parentPath ? data.parentPath : null);
		return promise;
	}

	save(id: string, requestBody: UpdateScriptRequestModel) {
		return this.#detailDataSource.update(id, requestBody);
	}

	async delete(id: string) {
		const promise = this.#detailDataSource.delete(id);
		const parentPath = id.substring(0, id.lastIndexOf('/'));
		this.#treeRepository.requestTreeItemsOf(parentPath ? parentPath : null);
		return promise;
	}

	requestItems(keys: Array<string>) {
		return this.#detailDataSource.getItems(keys);
	}

	//#endregion
}
