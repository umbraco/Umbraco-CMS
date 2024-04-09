/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { MemberValueModel } from './MemberValueModel';
import type { MemberVariantRequestModel } from './MemberVariantRequestModel';

export type UpdateMemberRequestModel = {
    values: Array<MemberValueModel>;
    variants: Array<MemberVariantRequestModel>;
    email: string;
    username: string;
    oldPassword?: string | null;
    newPassword?: string | null;
    groups?: Array<string> | null;
    isApproved: boolean;
    isLockedOut: boolean;
    isTwoFactorEnabled: boolean;
};

