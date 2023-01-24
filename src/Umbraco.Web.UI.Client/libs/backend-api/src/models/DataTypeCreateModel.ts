/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DataTypeProperty } from './DataTypeProperty';

export type DataTypeCreateModel = {
    name?: string;
    propertyEditorAlias?: string;
    propertyEditorUiAlias?: string | null;
    data?: Array<DataTypeProperty>;
    parentKey?: string | null;
};

