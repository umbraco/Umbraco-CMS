import type {
	TemplateQueryExecuteFilterPresentationModel,
	TemplateQueryExecuteSortModel,
} from '@umbraco-cms/backoffice/backend-api';

export interface UmbExecuteTemplateQueryArgs {
	rootDocument: { unique: string } | null;
	documentTypeAlias: string | null;
	filters: Array<TemplateQueryExecuteFilterPresentationModel> | null;
	sort: TemplateQueryExecuteSortModel | null;
	take: number;
}
