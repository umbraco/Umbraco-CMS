/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentForMemberResponseModel } from './ContentForMemberResponseModel';
import type { MemberTypeReferenceResponseModel } from './MemberTypeReferenceResponseModel';

export type MemberResponseModel = (ContentForMemberResponseModel & {
    email: string;
    username: string;
    memberType: MemberTypeReferenceResponseModel;
    isApproved: boolean;
    isLockedOut: boolean;
    isTwoFactorEnabled: boolean;
    failedPasswordAttempts: number;
    lastLoginDate?: string | null;
    lastLockoutDate?: string | null;
    lastPasswordChangeDate?: string | null;
    groups: Array<string>;
});

