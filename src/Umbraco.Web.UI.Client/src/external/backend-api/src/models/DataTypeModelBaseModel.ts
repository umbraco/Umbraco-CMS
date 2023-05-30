/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DataTypePropertyPresentationModel } from './DataTypePropertyPresentationModel';

export type DataTypeModelBaseModel = {
    name?: string;
    propertyEditorAlias?: string;
    propertyEditorUiAlias?: string | null;
    values?: Array<DataTypePropertyPresentationModel>;
};

