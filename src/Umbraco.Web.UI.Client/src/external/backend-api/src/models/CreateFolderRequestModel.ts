/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { FolderModelBaseModel } from './FolderModelBaseModel';

export type CreateFolderRequestModel = (FolderModelBaseModel & {
id?: string | null;
parentId?: string | null;
});
