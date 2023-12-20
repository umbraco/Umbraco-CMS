/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

export type UserGroupBaseModel = {
    name: string;
    icon?: string | null;
    sections: Array<string>;
    languages: Array<string>;
    hasAccessToAllLanguages: boolean;
    documentStartNodeId?: string | null;
    documentRootAccess: boolean;
    mediaStartNodeId?: string | null;
    mediaRootAccess: boolean;
    permissions: Array<string>;
};

