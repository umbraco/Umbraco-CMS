/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DocumentPermissionPresentationModel } from './DocumentPermissionPresentationModel';
import type { UnknownTypePermissionPresentationModel } from './UnknownTypePermissionPresentationModel';

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
    permissions: Array<(DocumentPermissionPresentationModel | UnknownTypePermissionPresentationModel)>;
};

