/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { FileSystemFolderModel } from './FileSystemFolderModel';

export type FileSystemTreeItemPresentationModel = {
    hasChildren: boolean;
    name: string;
    path: string;
    parent?: FileSystemFolderModel | null;
    isFolder: boolean;
};

