/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { TreeItemModel } from './TreeItemModel';

export type EntityTreeItemModel = (TreeItemModel & {
    $type: string;
    key?: string;
    isContainer?: boolean;
    parentKey?: string | null;
});

