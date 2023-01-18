/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { TemplateQueryOperator } from './TemplateQueryOperator';
import type { TemplateQueryProperty } from './TemplateQueryProperty';

export type TemplateQuerySettings = {
    contentTypeAliases?: Array<string>;
    properties?: Array<TemplateQueryProperty>;
    operators?: Array<TemplateQueryOperator>;
};

