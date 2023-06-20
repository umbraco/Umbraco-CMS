/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DataTypeModelBaseModel } from './DataTypeModelBaseModel';

export type CreateDataTypeRequestModel = (DataTypeModelBaseModel & {
id?: string | null;
parentId?: string | null;
});
