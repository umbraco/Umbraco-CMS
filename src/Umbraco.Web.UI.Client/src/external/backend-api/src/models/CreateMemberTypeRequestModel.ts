/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { CreateContentTypeForMemberTypeRequestModel } from './CreateContentTypeForMemberTypeRequestModel';
import type { MemberTypeCompositionModel } from './MemberTypeCompositionModel';

export type CreateMemberTypeRequestModel = (CreateContentTypeForMemberTypeRequestModel & {
    compositions: Array<MemberTypeCompositionModel>;
});

