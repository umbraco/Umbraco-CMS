/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { FolderModelBaseModel } from './FolderModelBaseModel';

export type CreateFolderRequestModel = (FolderModelBaseModel & {
parentKey?: string | null;
});
