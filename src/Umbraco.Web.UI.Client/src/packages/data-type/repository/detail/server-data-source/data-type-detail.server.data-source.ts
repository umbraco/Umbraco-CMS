import type { UmbDataTypeDetailModel, UmbDataTypePropertyValueModel } from '../../../types.js';
import { UMB_DATA_TYPE_ENTITY_TYPE } from '../../../entity.js';
import { UmbManagementApiDataTypeDetailDataRequestManager } from './data-type-detail.server.request-manager.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type {
	UmbDataSourceErrorResponse,
	UmbDataSourceResponse,
	UmbDetailDataSource,
} from '@umbraco-cms/backoffice/repository';
import type {
	CreateDataTypeRequestModel,
	DataTypeResponseModel,
	UpdateDataTypeRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

/**
 * A data source for the Data Type that fetches data from the server
 * @class UmbDataTypeServerDataSource
 * @implements {UmbDetailDataSource<UmbDataTypeDetailModel>}
 */
export class UmbDataTypeServerDataSource
	extends UmbControllerBase
	implements UmbDetailDataSource<UmbDataTypeDetailModel>
{
	#detailRequestManager = new UmbManagementApiDataTypeDetailDataRequestManager(this);
	#editorDataSourceAlias = 'umbEditorDataSource';

	/**
	 * Creates a new Data Type scaffold
	 * @param {Partial<UmbDataTypeDetailModel>} preset - The preset data to populate the scaffold with.
	 * @returns { CreateDataTypeRequestModel } The data type scaffold.
	 * @memberof UmbDataTypeServerDataSource
	 */
	async createScaffold(preset: Partial<UmbDataTypeDetailModel> = {}) {
		const data: UmbDataTypeDetailModel = {
			entityType: UMB_DATA_TYPE_ENTITY_TYPE,
			unique: UmbId.new(),
			name: '',
			editorAlias: undefined,
			editorUiAlias: null,
			values: [],
			...preset,
		};

		return { data };
	}

	/**
	 * Fetches a Data Type with the given id from the server
	 * @param {string} unique - The unique identifier of the data type to fetch.
	 * @returns {Promise<UmbDataSourceResponse<UmbDataTypeDetailModel>>} The data type.
	 * @memberof UmbDataTypeServerDataSource
	 */
	async read(unique: string): Promise<UmbDataSourceResponse<UmbDataTypeDetailModel>> {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await this.#detailRequestManager.read(unique);

		return { data: data ? this.#mapServerResponseModelToEntityDetailModel(data) : undefined, error };
	}

	/**
	 * Fetches multiple Data Types by their unique IDs from the server
	 * @param {Array<string>} uniques - The unique IDs of the data types to fetch
	 * @returns {Promise<UmbDataSourceResponse<Array<UmbDataTypeDetailModel>>>} The data types.
	 * @memberof UmbDataTypeServerDataSource
	 */
	async readMany(uniques: Array<string>): Promise<UmbDataSourceResponse<Array<UmbDataTypeDetailModel>>> {
		if (!uniques || uniques.length === 0) {
			return { data: [] };
		}

		const { data, error } = await this.#detailRequestManager.readMany(uniques);

		return {
			data: data?.items?.map((item) => this.#mapServerResponseModelToEntityDetailModel(item)),
			error,
		};
	}

	/**
	 * Inserts a new Data Type on the server
	 * @param {UmbDataTypeDetailModel} model - The data type to create.
	 * @param {string | null} parentUnique - The unique identifier of the parent, if any.
	 * @returns {Promise<UmbDataSourceResponse<UmbDataTypeDetailModel>>} The created data type.
	 * @memberof UmbDataTypeServerDataSource
	 */
	async create(
		model: UmbDataTypeDetailModel,
		parentUnique: string | null = null,
	): Promise<UmbDataSourceResponse<UmbDataTypeDetailModel>> {
		if (!model) throw new Error('Data Type is missing');
		if (!model.unique) throw new Error('Data Type unique is missing');
		if (!model.editorAlias) throw new Error('Property Editor Alias is missing');
		if (!model.editorUiAlias) throw new Error('Property Editor UI Alias is missing');

		const values = [...model.values];

		if (model.editorDataSourceAlias) {
			const editorDataSourceValue = {
				alias: this.#editorDataSourceAlias,
				value: model.editorDataSourceAlias,
			};

			values.unshift(editorDataSourceValue);
		}

		// TODO: make data mapper to prevent errors
		const body: CreateDataTypeRequestModel = {
			id: model.unique,
			parent: parentUnique ? { id: parentUnique } : null,
			name: model.name,
			editorAlias: model.editorAlias,
			editorUiAlias: model.editorUiAlias,
			values,
		};

		const { data, error } = await this.#detailRequestManager.create(body);

		return { data: data ? this.#mapServerResponseModelToEntityDetailModel(data) : undefined, error };
	}

	/**
	 * Updates a DataType on the server
	 * @param {UmbDataTypeDetailModel} model - The data type to update.
	 * @returns {Promise<UmbDataSourceResponse<UmbDataTypeDetailModel>>} The updated data type.
	 * @memberof UmbDataTypeServerDataSource
	 */
	async update(model: UmbDataTypeDetailModel): Promise<UmbDataSourceResponse<UmbDataTypeDetailModel>> {
		if (!model.unique) throw new Error('Unique is missing');
		if (!model.editorAlias) throw new Error('Property Editor Alias is missing');
		if (!model.editorUiAlias) throw new Error('Property Editor UI Alias is missing');

		const values = [...model.values];

		if (model.editorDataSourceAlias) {
			const editorDataSourceValue = {
				alias: this.#editorDataSourceAlias,
				value: model.editorDataSourceAlias,
			};

			values.unshift(editorDataSourceValue);
		}

		// TODO: make data mapper to prevent errors
		const body: UpdateDataTypeRequestModel = {
			name: model.name,
			editorAlias: model.editorAlias,
			editorUiAlias: model.editorUiAlias,
			values,
		};

		const { data, error } = await this.#detailRequestManager.update(model.unique, body);

		return { data: data ? this.#mapServerResponseModelToEntityDetailModel(data) : undefined, error };
	}

	/**
	 * Deletes a Data Type on the server
	 * @param {string} unique - The unique identifier of the data type to delete.
	 * @returns {Promise<UmbDataSourceErrorResponse>} The result of the delete operation.
	 * @memberof UmbDataTypeServerDataSource
	 */
	async delete(unique: string): Promise<UmbDataSourceErrorResponse> {
		if (!unique) throw new Error('Unique is missing');
		return this.#detailRequestManager.delete(unique);
	}

	// TODO: change this to a mapper extension when the endpoints returns a $type for DataTypeResponseModel
	#mapServerResponseModelToEntityDetailModel(data: DataTypeResponseModel): UmbDataTypeDetailModel {
		let values = data.values as Array<UmbDataTypePropertyValueModel>;
		const index = values?.findIndex((x) => x.alias === this.#editorDataSourceAlias);

		let editorDataSourceAlias;

		/* Remove the editorDataSourceAlias from the values collection
		 to prevent it from being treated as a regular config value. */
		if (index !== -1) {
			editorDataSourceAlias = values?.[index].value as string | null;
			values = values?.filter((value) => value.alias !== this.#editorDataSourceAlias) ?? [];
		}

		return {
			entityType: UMB_DATA_TYPE_ENTITY_TYPE,
			unique: data.id,
			name: data.name,
			editorAlias: data.editorAlias,
			editorUiAlias: data.editorUiAlias || null,
			editorDataSourceAlias: editorDataSourceAlias,
			values,
		};
	}
}
