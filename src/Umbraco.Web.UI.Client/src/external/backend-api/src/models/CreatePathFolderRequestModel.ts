/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { PathFolderModelBaseModel } from './PathFolderModelBaseModel';

export type CreatePathFolderRequestModel = (PathFolderModelBaseModel & {
    parentPath?: string | null;
});

