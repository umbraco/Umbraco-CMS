/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { TemplateQueryOperatorModel } from './TemplateQueryOperatorModel';
import type { TemplateQueryPropertyModel } from './TemplateQueryPropertyModel';

export type TemplateQuerySettingsModel = {
    contentTypeAliases?: Array<string>;
    properties?: Array<TemplateQueryPropertyModel>;
    operators?: Array<TemplateQueryOperatorModel>;
};

