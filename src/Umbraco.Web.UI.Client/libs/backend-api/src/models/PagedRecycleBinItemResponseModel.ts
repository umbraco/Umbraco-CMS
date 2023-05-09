/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { RecycleBinItemResponseModel } from './RecycleBinItemResponseModel';

export type PagedRecycleBinItemResponseModel = {
    total: number;
    items: Array<RecycleBinItemResponseModel>;
};

