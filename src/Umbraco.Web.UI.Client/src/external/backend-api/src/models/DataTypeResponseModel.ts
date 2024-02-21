/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DataTypeModelBaseModel } from './DataTypeModelBaseModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';

export type DataTypeResponseModel = (DataTypeModelBaseModel & {
    id: string;
    parent?: ReferenceByIdModel | null;
    isDeletable: boolean;
});

