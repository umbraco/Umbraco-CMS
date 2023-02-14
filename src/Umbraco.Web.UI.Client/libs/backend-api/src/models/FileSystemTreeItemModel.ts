/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { TreeItemModel } from './TreeItemModel';

export type FileSystemTreeItemModel = (TreeItemModel & {
    path?: string;
    isFolder?: boolean;
});

