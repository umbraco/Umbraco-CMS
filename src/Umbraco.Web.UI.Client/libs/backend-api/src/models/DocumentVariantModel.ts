/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentStateModel } from './ContentStateModel';
import type { VariantViewModelBaseModel } from './VariantViewModelBaseModel';

export type DocumentVariantModel = (VariantViewModelBaseModel & {
    state?: ContentStateModel;
    publishDate?: string | null;
});

