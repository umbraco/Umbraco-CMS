/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { EntityTreeItemModel } from './EntityTreeItemModel';

export type FolderTreeItemModel = (EntityTreeItemModel & {
    isFolder?: boolean;
});

