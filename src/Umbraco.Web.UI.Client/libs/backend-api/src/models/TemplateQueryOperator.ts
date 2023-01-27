/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { Operator } from './Operator';
import type { TemplateQueryPropertyType } from './TemplateQueryPropertyType';

export type TemplateQueryOperator = {
    operator?: Operator;
    applicableTypes?: Array<TemplateQueryPropertyType>;
};

