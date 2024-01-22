/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { FileSystemFolderModel } from './FileSystemFolderModel';
import type { TreeItemPresentationModel } from './TreeItemPresentationModel';

export type FileSystemTreeItemPresentationModel = (TreeItemPresentationModel & {
    path: string;
    parent?: FileSystemFolderModel | null;
    isFolder: boolean;
});

