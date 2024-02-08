/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { FolderModelBaseModel } from './FolderModelBaseModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';

export type CreateFolderRequestModel = (FolderModelBaseModel & {
    id?: string | null;
    parent?: ReferenceByIdModel | null;
});

