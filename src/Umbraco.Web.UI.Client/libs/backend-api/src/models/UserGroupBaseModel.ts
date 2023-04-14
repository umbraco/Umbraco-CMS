/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

export type UserGroupBaseModel = {
    name?: string;
    icon?: string | null;
    sections?: Array<string>;
    languages?: Array<string>;
    hasAccessToAllLanguages?: boolean;
    documentStartNodeId?: string | null;
    mediaStartNodeId?: string | null;
    permissions?: Array<string>;
};

