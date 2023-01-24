/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DataTypePropertyReference } from './DataTypePropertyReference';

export type DataTypeReference = {
    key?: string;
    type?: string;
    properties?: Array<DataTypePropertyReference>;
};

