/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DataTypeProperty } from './DataTypeProperty';

export type DataType = {
    name?: string | null;
    propertyEditorAlias?: string | null;
    data?: Array<DataTypeProperty> | null;
    key?: string;
    parentKey?: string | null;
};

