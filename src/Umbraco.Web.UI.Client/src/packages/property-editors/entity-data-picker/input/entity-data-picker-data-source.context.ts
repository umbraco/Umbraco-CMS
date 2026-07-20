import { UMB_ENTITY_DATA_PICKER_DATA_SOURCE_API_CONTEXT } from './entity-data-picker-data-source.context.token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBasicState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbPickerDataSource } from '@umbraco-cms/backoffice/picker-data-source';

export class UmbEntityDataPickerDataSourceApiContext<
	DataSourceApiType extends UmbPickerDataSource,
> extends UmbContextBase {
	#dataSourceApi = new UmbBasicState<DataSourceApiType | undefined>(undefined);
	public readonly dataSourceApi = this.#dataSourceApi.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_ENTITY_DATA_PICKER_DATA_SOURCE_API_CONTEXT);
	}

	setDataSourceApi(dataSourceApi: DataSourceApiType | undefined) {
		this.#dataSourceApi.setValue(dataSourceApi);
	}

	getDataSourceApi(): DataSourceApiType | undefined {
		return this.#dataSourceApi.getValue();
	}
}
