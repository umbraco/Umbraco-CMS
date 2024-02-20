/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DocumentPermissionModel } from './DocumentPermissionModel';
import type { GlobalPermissionModel } from './GlobalPermissionModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';

export type UserGroupBaseModel = {
    name: string;
    icon?: string | null;
    sections: Array<string>;
    languages: Array<string>;
    hasAccessToAllLanguages: boolean;
    documentStartNode?: ReferenceByIdModel | null;
    documentRootAccess: boolean;
    mediaStartNode?: ReferenceByIdModel | null;
    mediaRootAccess: boolean;
    permissions: Array<(DocumentPermissionModel | GlobalPermissionModel)>;
};

