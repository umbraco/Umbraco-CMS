/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ReferenceByIdModel } from './ReferenceByIdModel';
import type { TemplateQueryExecuteFilterPresentationModel } from './TemplateQueryExecuteFilterPresentationModel';
import type { TemplateQueryExecuteSortModel } from './TemplateQueryExecuteSortModel';

export type TemplateQueryExecuteModel = {
    rootDocument?: ReferenceByIdModel | null;
    documentTypeAlias?: string | null;
    filters?: Array<TemplateQueryExecuteFilterPresentationModel> | null;
    sort?: TemplateQueryExecuteSortModel | null;
    take: number;
};

