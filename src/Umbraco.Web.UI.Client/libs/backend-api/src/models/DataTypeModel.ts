/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DataTypeModelBaseModel } from './DataTypeModelBaseModel';

export type DataTypeModel = (DataTypeModelBaseModel & {
    $type: string;
    key?: string;
    parentKey?: string | null;
});

