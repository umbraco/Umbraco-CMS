/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { UserStateModel } from './UserStateModel';

export type UserResponseModel = {
    email: string;
    userName: string;
    name: string;
    userGroupIds: Array<string>;
    id: string;
    languageIsoCode?: string | null;
    documentStartNodeIds: Array<string>;
    mediaStartNodeIds: Array<string>;
    avatarUrls: Array<string>;
    state: UserStateModel;
    failedLoginAttempts: number;
    createDate: string;
    updateDate: string;
    lastLoginDate?: string | null;
    lastLockoutDate?: string | null;
    lastPasswordChangeDate?: string | null;
    isAdmin: boolean;
};

