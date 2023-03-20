/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { TreeItemPresentationModel } from './TreeItemPresentationModel';

export type EntityTreeItemResponseModel = (TreeItemPresentationModel & {
$type: string;
key?: string;
isContainer?: boolean;
parentKey?: string | null;
});
