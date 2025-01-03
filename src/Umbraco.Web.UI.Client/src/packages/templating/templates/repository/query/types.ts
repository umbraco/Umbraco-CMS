import type {
	TemplateQueryExecuteFilterPresentationModel,
	TemplateQueryExecuteSortModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export interface UmbExecuteTemplateQueryRequestModel {
	rootDocument: { unique: string } | null;
	documentTypeAlias: string | null;
	filters: Array<TemplateQueryExecuteFilterPresentationModel> | null;
	sort: TemplateQueryExecuteSortModel | null;
	take: number;
}
