/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { FieldModel } from './FieldModel';

export type SearchResultModel = {
    id?: string;
    score?: number;
    readonly fieldCount?: number;
    fields?: Array<FieldModel>;
};
