import type { UmbDataTypeDetailModel, UmbDataTypePropertyValueModel } from '../../types.js';
import { UMB_DATA_TYPE_ENTITY_TYPE } from '../../entity.js';
import { cache } from './data-type-detail.server.runtime-cache.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type {
	CreateDataTypeRequestModel,
	DataTypeResponseModel,
	UpdateDataTypeRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { DataTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute, UmbError } from '@umbraco-cms/backoffice/resources';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT } from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A data source for the Data Type that fetches data from the server
 * @class UmbDataTypeServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDataTypeServerDataSource
	extends UmbControllerBase
	implements UmbDetailDataSource<UmbDataTypeDetailModel>
{
	#runtimeCache = cache;
	#serverEventContext?: typeof UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT.TYPE;
	#eventSource = 'Umbraco:CMS:DataType';

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT, (context) => {
			this.#serverEventContext = context;
			this.#observeServerEvents();
		});
	}

	#observeServerEvents() {
		this.observe(
			this.#serverEventContext?.byEventSourceAndTypes(this.#eventSource, ['Updated', 'Deleted']),
			(event) => {
				if (!event) return;
				this.#runtimeCache.delete(event.key);
			},
		);
	}

	/**
	 * Creates a new Data Type scaffold
	 * @param {(string | null)} parentUnique
	 * @param preset
	 * @returns { CreateDataTypeRequestModel }
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
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbDataTypeServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		let data: DataTypeResponseModel | undefined;

		if (this.#runtimeCache.has(unique)) {
			data = this.#runtimeCache.get(unique);
		} else {
			const { data: serverData, error: serverError } = await tryExecute(
				this,
				DataTypeService.getDataTypeById({ path: { id: unique } }),
			);

			if (serverError || !serverData) {
				return { error: serverError };
			}

			this.#runtimeCache.set(unique, serverData);
			data = serverData;
		}

		if (!data) {
			return { error: new UmbError(`Data Type with unique "${unique}" not found.`) };
		}

		// TODO: make data mapper to prevent errors
		const dataType: UmbDataTypeDetailModel = {
			entityType: UMB_DATA_TYPE_ENTITY_TYPE,
			unique: data.id,
			name: data.name,
			editorAlias: data.editorAlias,
			editorUiAlias: data.editorUiAlias || null,
			values: data.values as Array<UmbDataTypePropertyValueModel>,
		};

		return { data: dataType };
	}

	/**
	 * Inserts a new Data Type on the server
	 * @param {UmbDataTypeDetailModel} model
	 * @param parentUnique
	 * @returns {*}
	 * @memberof UmbDataTypeServerDataSource
	 */
	async create(model: UmbDataTypeDetailModel, parentUnique: string | null = null) {
		if (!model) throw new Error('Data Type is missing');
		if (!model.unique) throw new Error('Data Type unique is missing');
		if (!model.editorAlias) throw new Error('Property Editor Alias is missing');
		if (!model.editorUiAlias) throw new Error('Property Editor UI Alias is missing');

		// TODO: make data mapper to prevent errors
		const body: CreateDataTypeRequestModel = {
			id: model.unique,
			parent: parentUnique ? { id: parentUnique } : null,
			name: model.name,
			editorAlias: model.editorAlias,
			editorUiAlias: model.editorUiAlias,
			values: model.values,
		};

		const { data, error } = await tryExecute(
			this,
			DataTypeService.postDataType({
				body: body,
			}),
		);

		if (data) {
			return this.read(data as any);
		}

		return { error };
	}

	/**
	 * Updates a DataType on the server
	 * @param {UmbDataTypeDetailModel} DataType
	 * @param model
	 * @returns {*}
	 * @memberof UmbDataTypeServerDataSource
	 */
	async update(model: UmbDataTypeDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');
		if (!model.editorAlias) throw new Error('Property Editor Alias is missing');
		if (!model.editorUiAlias) throw new Error('Property Editor UI Alias is missing');

		// TODO: make data mapper to prevent errors
		const body: UpdateDataTypeRequestModel = {
			name: model.name,
			editorAlias: model.editorAlias,
			editorUiAlias: model.editorUiAlias,
			values: model.values,
		};

		const { error } = await tryExecute(
			this,
			DataTypeService.putDataTypeById({
				path: { id: model.unique },
				body: body,
			}),
		);

		if (!error) {
			return this.read(model.unique);
		}

		return { error };
	}

	/**
	 * Deletes a Data Type on the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbDataTypeServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { error } = await tryExecute(
			this,
			DataTypeService.deleteDataTypeById({
				path: { id: unique },
			}),
		);

		if (!error) {
			this.#runtimeCache.delete(unique);
		}

		return { error };
	}
}
