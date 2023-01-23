/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { TemplateQueryResultItem } from './TemplateQueryResultItem';

export type TemplateQueryResult = {
    queryExpression?: string;
    sampleResults?: Array<TemplateQueryResultItem>;
    resultCount?: number;
    executionTime?: number;
};

