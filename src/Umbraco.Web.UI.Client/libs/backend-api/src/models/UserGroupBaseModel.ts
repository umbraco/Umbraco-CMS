/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

export type UserGroupBaseModel = {
    name?: string;
    icon?: string | null;
    sections?: Array<string>;
    languages?: Array<string>;
    hasAccessToAllLanguages?: boolean;
    documentStartNodeKey?: string | null;
    mediaStartNodeKey?: string | null;
    permissions?: Array<string>;
};

