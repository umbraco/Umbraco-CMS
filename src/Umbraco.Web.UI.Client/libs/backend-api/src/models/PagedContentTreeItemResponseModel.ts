/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentTreeItemResponseModel } from './ContentTreeItemResponseModel';
import type { DocumentTreeItemResponseModel } from './DocumentTreeItemResponseModel';

export type PagedContentTreeItemResponseModel = {
    total: number;
    items: Array<(ContentTreeItemResponseModel | DocumentTreeItemResponseModel)>;
};
