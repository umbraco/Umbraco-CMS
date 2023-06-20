/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { FileSystemTreeItemPresentationModel } from './FileSystemTreeItemPresentationModel';

export type PagedFileSystemTreeItemPresentationModel = {
    total: number;
    items: Array<FileSystemTreeItemPresentationModel>;
};

