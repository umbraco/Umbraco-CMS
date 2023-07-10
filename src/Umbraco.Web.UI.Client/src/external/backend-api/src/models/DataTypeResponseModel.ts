/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DataTypeModelBaseModel } from './DataTypeModelBaseModel';

export type DataTypeResponseModel = (DataTypeModelBaseModel & {
    id?: string;
    parentId?: string | null;
});

