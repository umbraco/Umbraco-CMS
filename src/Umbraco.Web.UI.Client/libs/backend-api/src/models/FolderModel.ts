/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { FolderModelBaseModel } from './FolderModelBaseModel';

export type FolderModel = (FolderModelBaseModel & {
    $type: string;
    key?: string;
    parentKey?: string | null;
});

