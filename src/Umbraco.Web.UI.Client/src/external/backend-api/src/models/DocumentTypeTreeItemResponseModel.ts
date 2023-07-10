/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { FolderTreeItemResponseModel } from './FolderTreeItemResponseModel';

export type DocumentTypeTreeItemResponseModel = (FolderTreeItemResponseModel & {
    isElement?: boolean;
    icon?: string;
});

