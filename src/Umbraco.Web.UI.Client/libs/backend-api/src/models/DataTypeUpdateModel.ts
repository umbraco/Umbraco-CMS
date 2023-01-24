/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DataTypeProperty } from './DataTypeProperty';

export type DataTypeUpdateModel = {
    name?: string;
    propertyEditorAlias?: string;
    propertyEditorUiAlias?: string | null;
    data?: Array<DataTypeProperty>;
};

