/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DocumentTypeTreeItemModel } from './DocumentTypeTreeItemModel';
import type { FolderTreeItemModel } from './FolderTreeItemModel';

export type PagedFolderTreeItemModel = {
    total: number;
    items: Array<(FolderTreeItemModel | DocumentTypeTreeItemModel)>;
};

