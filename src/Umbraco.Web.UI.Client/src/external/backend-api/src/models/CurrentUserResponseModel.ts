/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DocumentPermissionModel } from './DocumentPermissionModel';
import type { FallbackPermissionModel } from './FallbackPermissionModel';
import type { UnknownTypePermissionModel } from './UnknownTypePermissionModel';

export type CurrentUserResponseModel = {
    id: string;
    email: string;
    userName: string;
    name: string;
    languageIsoCode?: string | null;
    documentStartNodeIds: Array<string>;
    mediaStartNodeIds: Array<string>;
    avatarUrls: Array<string>;
    languages: Array<string>;
    hasAccessToAllLanguages: boolean;
    permissions: Array<(DocumentPermissionModel | FallbackPermissionModel | UnknownTypePermissionModel)>;
};

