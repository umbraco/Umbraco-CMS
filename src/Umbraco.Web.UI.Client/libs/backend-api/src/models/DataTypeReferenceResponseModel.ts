/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DataTypePropertyReferenceModel } from './DataTypePropertyReferenceModel';

export type DataTypeReferenceResponseModel = {
    key?: string;
    type?: string;
    properties?: Array<DataTypePropertyReferenceModel>;
};
