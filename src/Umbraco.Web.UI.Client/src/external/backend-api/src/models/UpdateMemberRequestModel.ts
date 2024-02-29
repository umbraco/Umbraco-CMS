/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { UpdateContentForMemberRequestModel } from './UpdateContentForMemberRequestModel';
export type UpdateMemberRequestModel = (UpdateContentForMemberRequestModel & {
    email: string;
    username: string;
    oldPassword?: string | null;
    newPassword?: string | null;
    groups?: Array<string> | null;
    isApproved: boolean;
    isLockedOut: boolean;
    isTwoFactorEnabled: boolean;
});

