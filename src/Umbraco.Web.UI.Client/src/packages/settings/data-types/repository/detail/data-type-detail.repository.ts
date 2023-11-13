import { UmbDataTypeServerDataSource } from './data-type-detail.server.data-source.js';
import { UmbDataTypeRepositoryBase } from '../data-type-repository-base.js';
import { createTreeItem } from '../utils.js';
import type { UmbDetailRepository, UmbDataSource } from '@umbraco-cms/backoffice/repository';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	CreateDataTypeRequestModel,
	DataTypeResponseModel,
	UpdateDataTypeRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
export class UmbDataTypeDetailRepository
	extends UmbDataTypeRepositoryBase
	implements UmbDetailRepository<CreateDataTypeRequestModel, any, UpdateDataTypeRequestModel, DataTypeResponseModel>
{
	#detailSource: UmbDataSource<CreateDataTypeRequestModel, any, UpdateDataTypeRequestModel, DataTypeResponseModel>;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#detailSource = new UmbDataTypeServerDataSource(this);
	}

	async createScaffold(parentId: string | null) {
		if (parentId === undefined) throw new Error('Parent id is missing');
		await this._init;

		return this.#detailSource.createScaffold(parentId);
	}

	async requestById(id: string) {
		if (!id) throw new Error('Key is missing');
		await this._init;

		const { data, error } = await this.#detailSource.get(id);

		if (data) {
			this._detailStore!.append(data);
		}

		return { data, error, asObservable: () => this._detailStore!.byId(id) };
	}

	async byId(id: string) {
		if (!id) throw new Error('Key is missing');
		await this._init;
		return this._detailStore!.byId(id);
	}

	async byPropertyEditorUiAlias(propertyEditorUiAlias: string) {
		if (!propertyEditorUiAlias) throw new Error('propertyEditorUiAlias is missing');
		await this._init;
		return this._detailStore!.withPropertyEditorUiAlias(propertyEditorUiAlias);
	}

	async create(dataType: CreateDataTypeRequestModel) {
		if (!dataType) throw new Error('Data Type is missing');
		if (!dataType.id) throw new Error('Data Type id is missing');
		await this._init;

		const { error } = await this.#detailSource.insert(dataType);

		if (!error) {
			// TODO: We need to push a new item to the tree store to update the tree. How do we want to create the tree items?
			const treeItem = createTreeItem(dataType);
			this._treeStore!.appendItems([treeItem]);
			//this.#detailStore?.append(dataType);

			const notification = { data: { message: `Data Type created` } };
			this._notificationContext!.peek('positive', notification);
		}

		return { error };
	}

	async save(id: string, updatedDataType: UpdateDataTypeRequestModel) {
		if (!id) throw new Error('Data Type id is missing');
		if (!updatedDataType) throw new Error('Data Type is missing');
		await this._init;

		const { error } = await this.#detailSource.update(id, updatedDataType);

		if (!error) {
			// TODO: we currently don't use the detail store for anything.
			// Consider to look up the data before fetching from the server
			// Consider notify a workspace if a template is updated in the store while someone is editing it.
			// TODO: would be nice to align the stores on methods/methodNames.
			this._detailStore!.updateItem(id, updatedDataType);
			// TODO: This is parsing on the full models to the tree and item store. Those should only contain the data they need. I don't know, at this point, if thats a repository or store responsibility.
			this._treeStore!.updateItem(id, updatedDataType);
			this._itemStore!.updateItem(id, updatedDataType);

			const notification = { data: { message: `Data Type saved` } };
			this._notificationContext!.peek('positive', notification);
		}

		return { error };
	}

	async delete(id: string) {
		if (!id) throw new Error('Data Type id is missing');
		await this._init;

		const { error } = await this.#detailSource.delete(id);

		if (!error) {
			// TODO: we currently don't use the detail store for anything.
			// Consider to look up the data before fetching from the server.
			// Consider notify a workspace if a template is deleted from the store while someone is editing it.
			// TODO: would be nice to align the stores on methods/methodNames.
			this._detailStore!.remove([id]);
			this._treeStore!.removeItem(id);
			this._itemStore!.removeItem(id);

			const notification = { data: { message: `Data Type deleted` } };
			this._notificationContext!.peek('positive', notification);
		}

		return { error };
	}
}
