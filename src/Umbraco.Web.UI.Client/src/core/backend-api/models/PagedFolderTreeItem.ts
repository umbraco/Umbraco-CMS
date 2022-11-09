/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { FolderTreeItem } from './FolderTreeItem';

export type PagedFolderTreeItem = {
    total?: number;
    items?: Array<FolderTreeItem> | null;
};

