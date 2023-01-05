/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DataTypeProperty } from './DataTypeProperty';

export type DataTypeUpdateModel = {
    name?: string | null;
    propertyEditorAlias?: string | null;
    data?: Array<DataTypeProperty> | null;
};

