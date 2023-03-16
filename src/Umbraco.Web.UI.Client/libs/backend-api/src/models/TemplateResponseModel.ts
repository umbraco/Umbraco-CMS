/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { TemplateModelBaseModel } from './TemplateModelBaseModel';

export type TemplateResponseModel = (TemplateModelBaseModel & {
$type: string;
key?: string;
});
