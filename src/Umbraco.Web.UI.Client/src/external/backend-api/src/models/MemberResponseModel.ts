/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { MemberTypeReferenceResponseModel } from './MemberTypeReferenceResponseModel';
import type { MemberValueModel } from './MemberValueModel';
import type { MemberVariantResponseModel } from './MemberVariantResponseModel';

export type MemberResponseModel = {
    values: Array<MemberValueModel>;
    variants: Array<MemberVariantResponseModel>;
    id: string;
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
};

