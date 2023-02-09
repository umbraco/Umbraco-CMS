/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DataTypePropertyModel } from './DataTypePropertyModel';

export type DataTypeModelBaseModel = {
    name?: string;
    propertyEditorAlias?: string;
    propertyEditorUiAlias?: string | null;
    data?: Array<DataTypePropertyModel>;
};

