/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ReferenceByIdModel } from './ReferenceByIdModel';
import type { TemplateModelBaseModel } from './TemplateModelBaseModel';

export type TemplateResponseModel = (TemplateModelBaseModel & {
    id: string;
    masterTemplate?: ReferenceByIdModel | null;
});

