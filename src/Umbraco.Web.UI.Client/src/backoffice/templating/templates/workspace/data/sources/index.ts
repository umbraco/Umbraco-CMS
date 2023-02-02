import { Template } from '@umbraco-cms/backend-api';
import type { DataSourceResponse } from '@umbraco-cms/models';

export interface TemplateDetailDataSource {
	createScaffold(parentKey: string | null): Promise<DataSourceResponse<Template>>;
	get(key: string): Promise<DataSourceResponse<Template>>;
	insert(template: Template): Promise<DataSourceResponse>;
	update(template: Template): Promise<DataSourceResponse>;
	delete(key: string): Promise<DataSourceResponse>;
}
