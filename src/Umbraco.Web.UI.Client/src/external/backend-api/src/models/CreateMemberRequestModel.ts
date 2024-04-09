/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { MemberValueModel } from './MemberValueModel';
import type { MemberVariantRequestModel } from './MemberVariantRequestModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';

export type CreateMemberRequestModel = {
    values: Array<MemberValueModel>;
    variants: Array<MemberVariantRequestModel>;
    id?: string | null;
    email: string;
    username: string;
    password: string;
    memberType: ReferenceByIdModel;
    groups?: Array<string> | null;
    isApproved: boolean;
};

