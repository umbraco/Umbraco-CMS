/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DocumentPermissionPresentationModel } from './DocumentPermissionPresentationModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';
import type { UnknownTypePermissionPresentationModel } from './UnknownTypePermissionPresentationModel';
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
    fallbackPermissions: Array<string>;
    permissions: Array<(DocumentPermissionPresentationModel | UnknownTypePermissionPresentationModel)>;
};

