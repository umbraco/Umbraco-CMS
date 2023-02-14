/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { TemplateQueryExecuteFilterModel } from './TemplateQueryExecuteFilterModel';
import type { TemplateQueryExecuteSortModel } from './TemplateQueryExecuteSortModel';

export type TemplateQueryExecuteModel = {
    rootContentKey?: string | null;
    contentTypeAlias?: string | null;
    filters?: Array<TemplateQueryExecuteFilterModel> | null;
    sort?: TemplateQueryExecuteSortModel | null;
    take?: number;
};
