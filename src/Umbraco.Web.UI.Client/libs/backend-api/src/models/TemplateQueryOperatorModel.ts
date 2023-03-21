/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { OperatorModel } from './OperatorModel';
import type { TemplateQueryPropertyTypeModel } from './TemplateQueryPropertyTypeModel';

export type TemplateQueryOperatorModel = {
    operator?: OperatorModel;
    applicableTypes?: Array<TemplateQueryPropertyTypeModel>;
};
