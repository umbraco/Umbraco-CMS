/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentStateModel } from './ContentStateModel';
import type { VariantResponseModelBaseModel } from './VariantResponseModelBaseModel';

export type DocumentVariantResponseModel = (VariantResponseModelBaseModel & {
$type: string;
state?: ContentStateModel;
publishDate?: string | null;
});
