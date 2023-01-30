import { EntityTreeItem, PagedEntityTreeItem, ProblemDetails, Template } from '@umbraco-cms/backend-api';

export interface DataOrErrorResponse<T> {
	data?: T;
	error?: ProblemDetails;
}

export interface ErrorResponse {
	data?: undefined;
	error?: ProblemDetails;
}

export interface TemplateDataSource {
	createScaffold(parentKey: string | null): Promise<DataOrErrorResponse<Template>>;
	get(key: string): Promise<DataOrErrorResponse<Template>>;
	insert(template: Template): Promise<ErrorResponse>;
	update(template: Template): Promise<ErrorResponse>;
	delete(key: string): Promise<ErrorResponse>;
	getTreeRoot(): Promise<DataOrErrorResponse<PagedEntityTreeItem>>;
	getTreeItemChildren(parentKey: string): Promise<DataOrErrorResponse<PagedEntityTreeItem>>;
	getTreeItems(key: Array<string>): Promise<DataOrErrorResponse<EntityTreeItem[]>>;
}
