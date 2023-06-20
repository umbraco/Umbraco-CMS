/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { FolderModelBaseModel } from './FolderModelBaseModel';

export type FolderReponseModel = (FolderModelBaseModel & {
    $type: string;
    id?: string;
    parentId?: string | null;
});

