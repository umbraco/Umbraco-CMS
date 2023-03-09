/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentTreeItemModel } from './ContentTreeItemModel';
import type { DocumentTreeItemModel } from './DocumentTreeItemModel';

export type PagedContentTreeItemModel = {
    total: number;
    items: Array<(ContentTreeItemModel | DocumentTreeItemModel)>;
};

