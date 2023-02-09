/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { FolderTreeItemModel } from './FolderTreeItemModel';

export type DocumentTypeTreeItemModel = (FolderTreeItemModel & {
    isElement?: boolean;
});

