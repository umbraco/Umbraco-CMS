/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { TemplateQueryResultItemPresentationModel } from './TemplateQueryResultItemPresentationModel';

export type TemplateQueryResultResponseModel = {
    queryExpression?: string;
    sampleResults?: Array<TemplateQueryResultItemPresentationModel>;
    resultCount?: number;
    executionTime?: number;
};

