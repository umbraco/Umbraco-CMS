/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { TreeItemPresentationModel } from './TreeItemPresentationModel';

export type FileSystemTreeItemPresentationModel = (TreeItemPresentationModel & {
path?: string;
isFolder?: boolean;
});
