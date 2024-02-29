/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreateContentForMemberRequestModel } from './CreateContentForMemberRequestModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';
export type CreateMemberRequestModel = (CreateContentForMemberRequestModel & {
    email: string;
    username: string;
    password: string;
    memberType: ReferenceByIdModel;
    groups?: Array<string> | null;
    isApproved: boolean;
});

