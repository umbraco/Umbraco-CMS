import { UmbScriptTreeRepository } from '../tree/index.js';
import { UmbScriptDetailServerDataSource } from './script-detail.server.data.js';
import { UmbDetailRepository } from '@umbraco-cms/backoffice/repository';
import {
	CreateScriptRequestModel,
	ScriptResponseModel,
	UpdateScriptRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbScriptDetailRepository
	extends UmbBaseController
	implements
		UmbDetailRepository<CreateScriptRequestModel, string, UpdateScriptRequestModel, ScriptResponseModel, string>,
		UmbApi
{
	#detailDataSource: UmbScriptDetailServerDataSource;
	// TODO: temp solution until it is automated
	#treeRepository = new UmbScriptTreeRepository(this);

	constructor(host: UmbControllerHost) {
		super(host);
		this.#detailDataSource = new UmbScriptDetailServerDataSource(this);
	}

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

	createScaffold(parentUnique: string | null, preset: string) {
		if (parentUnique === undefined) throw new Error('Parent unique is missing');
		return this.#detailDataSource.createScaffold(parentUnique, preset);
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
