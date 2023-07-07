/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentStateModel } from './ContentStateModel';
import type { VariantResponseModelBaseModel } from './VariantResponseModelBaseModel';

export type DocumentVariantResponseModel = (VariantResponseModelBaseModel & {
    state?: ContentStateModel;
    publishDate?: string | null;
});

