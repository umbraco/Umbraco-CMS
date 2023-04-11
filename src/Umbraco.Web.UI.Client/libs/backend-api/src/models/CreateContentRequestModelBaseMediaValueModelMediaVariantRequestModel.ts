/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { MediaValueModel } from './MediaValueModel';
import type { MediaVariantRequestModel } from './MediaVariantRequestModel';

export type CreateContentRequestModelBaseMediaValueModelMediaVariantRequestModel = {
    values?: Array<MediaValueModel>;
    variants?: Array<MediaVariantRequestModel>;
    parentId?: string | null;
};

