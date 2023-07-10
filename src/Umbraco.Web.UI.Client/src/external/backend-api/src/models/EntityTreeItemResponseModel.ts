/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { TreeItemPresentationModel } from './TreeItemPresentationModel';

export type EntityTreeItemResponseModel = (TreeItemPresentationModel & {
    id?: string;
    isContainer?: boolean;
    parentId?: string | null;
});

