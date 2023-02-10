import type { TemplateModel } from '@umbraco-cms/backend-api';
import type { DataSourceResponse } from '@umbraco-cms/models';

export interface TemplateDetailDataSource {
	createScaffold(): Promise<DataSourceResponse<TemplateModel>>;
	get(key: string): Promise<DataSourceResponse<TemplateModel>>;
	insert(template: TemplateModel): Promise<DataSourceResponse>;
	update(template: TemplateModel): Promise<DataSourceResponse>;
	delete(key: string): Promise<DataSourceResponse>;
}
