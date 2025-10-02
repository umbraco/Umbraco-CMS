import { UMB_ENTITY_DATA_PICKER_DATA_SOURCE_API_CONTEXT } from './entity-data-picker-data-source.context.token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbPickerPropertyEditorDataSource } from '@umbraco-cms/backoffice/data-type';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

export class UmbEntityDataPickerDataSourceApiContext<
	DataSourceApiType extends UmbPickerPropertyEditorDataSource,
> extends UmbContextBase {
	#dataSourceApi?: DataSourceApiType;
	#config?: UmbPropertyEditorConfigCollection | undefined;

	constructor(host: UmbControllerHost) {
		super(host, UMB_ENTITY_DATA_PICKER_DATA_SOURCE_API_CONTEXT);
	}

	setDataSourceApi(dataSourceApi: DataSourceApiType) {
		this.#dataSourceApi = dataSourceApi;
	}

	getDataSourceApi() {
		return this.#dataSourceApi;
	}

	setConfig(config: UmbPropertyEditorConfigCollection | undefined) {
		this.#config = config;
	}

	getConfig(): UmbPropertyEditorConfigCollection | undefined {
		return this.#config;
	}
}
