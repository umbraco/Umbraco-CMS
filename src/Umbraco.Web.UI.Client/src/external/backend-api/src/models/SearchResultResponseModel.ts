/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { FieldPresentationModel } from './FieldPresentationModel';

export type SearchResultResponseModel = {
    id?: string;
    score?: number;
    readonly fieldCount?: number;
    fields?: Array<FieldPresentationModel>;
};
