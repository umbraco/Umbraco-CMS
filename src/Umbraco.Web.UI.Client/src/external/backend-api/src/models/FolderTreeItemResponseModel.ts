/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { EntityTreeItemResponseModel } from './EntityTreeItemResponseModel';

export type FolderTreeItemResponseModel = (EntityTreeItemResponseModel & {
    isFolder?: boolean;
});

