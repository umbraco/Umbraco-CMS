import { UMB_ENTITY_DATA_PICKER_DATA_SOURCE_API_CONTEXT } from './entity-data-picker-data-source.context.token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbPickerPropertyEditorDataSource,
	UmbPropertyEditorDataSourceConfigModel,
} from '@umbraco-cms/backoffice/data-type';

export class UmbEntityDataPickerDataSourceApiContext<
	DataSourceApiType extends UmbPickerPropertyEditorDataSource,
> extends UmbContextBase {
	#dataSourceApi?: DataSourceApiType;
	#config?: UmbPropertyEditorDataSourceConfigModel | undefined;

	constructor(host: UmbControllerHost) {
		super(host, UMB_ENTITY_DATA_PICKER_DATA_SOURCE_API_CONTEXT);
	}

	setDataSourceApi(dataSourceApi: DataSourceApiType) {
		this.#dataSourceApi = dataSourceApi;
	}

	getDataSourceApi(): DataSourceApiType | undefined {
		return this.#dataSourceApi;
	}

	setConfig(config: UmbPropertyEditorDataSourceConfigModel | undefined) {
		this.#config = config;
	}

	getConfig(): UmbPropertyEditorDataSourceConfigModel | undefined {
		return this.#config;
	}
}
