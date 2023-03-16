/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DocumentTypeTreeItemResponseModel } from './DocumentTypeTreeItemResponseModel';
import type { FolderTreeItemResponseModel } from './FolderTreeItemResponseModel';

export type PagedFolderTreeItemResponseModel = {
    total: number;
    items: Array<(FolderTreeItemResponseModel | DocumentTypeTreeItemResponseModel)>;
};
