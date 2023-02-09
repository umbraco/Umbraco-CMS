/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { TemplateQueryResultItemModel } from './TemplateQueryResultItemModel';

export type TemplateQueryResultModel = {
    queryExpression?: string;
    sampleResults?: Array<TemplateQueryResultItemModel>;
    resultCount?: number;
    executionTime?: number;
};

