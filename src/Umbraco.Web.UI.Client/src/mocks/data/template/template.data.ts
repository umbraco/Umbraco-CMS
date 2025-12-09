import { dataSet } from '../sets/index.js';
import type {
	TemplateResponseModel,
	TemplateItemResponseModel,
	TemplateQuerySettingsResponseModel,
	TemplateQueryResultResponseModel,
	NamedEntityTreeItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockTemplateModel = TemplateResponseModel & NamedEntityTreeItemResponseModel & TemplateItemResponseModel;

export const data: Array<UmbMockTemplateModel> = dataSet.template;

export const createTemplateScaffold = dataSet.createTemplateScaffold;

export const templateQueryResult: TemplateQueryResultResponseModel = dataSet.templateQueryResult;

export const templateQuerySettings: TemplateQuerySettingsResponseModel = dataSet.templateQuerySettings;
