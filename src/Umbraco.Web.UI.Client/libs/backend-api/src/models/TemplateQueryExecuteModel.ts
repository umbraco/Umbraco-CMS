/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { TemplateQueryExecuteFilterPresentationModel } from './TemplateQueryExecuteFilterPresentationModel';
import type { TemplateQueryExecuteSortModel } from './TemplateQueryExecuteSortModel';

export type TemplateQueryExecuteModel = {
    rootContentId?: string | null;
    contentTypeAlias?: string | null;
    filters?: Array<TemplateQueryExecuteFilterPresentationModel> | null;
    sort?: TemplateQueryExecuteSortModel | null;
    take?: number;
};
