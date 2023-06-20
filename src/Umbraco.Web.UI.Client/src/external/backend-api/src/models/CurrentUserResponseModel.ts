/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

export type CurrentUserResponseModel = {
    $type: string;
    id?: string;
    email?: string;
    userName?: string;
    name?: string;
    languageIsoCode?: string | null;
    contentStartNodeIds?: Array<string>;
    mediaStartNodeIds?: Array<string>;
    avatarUrls?: Array<string>;
    languages?: Array<string>;
    hasAccessToAllLanguages?: boolean;
    permissions?: Array<string>;
};

