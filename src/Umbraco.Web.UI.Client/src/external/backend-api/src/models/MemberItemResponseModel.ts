/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { MemberTypeReferenceResponseModel } from './MemberTypeReferenceResponseModel';
import type { NamedItemResponseModelBaseModel } from './NamedItemResponseModelBaseModel';
import type { VariantItemResponseModel } from './VariantItemResponseModel';

export type MemberItemResponseModel = (NamedItemResponseModelBaseModel & {
    memberType: MemberTypeReferenceResponseModel;
    variants: Array<VariantItemResponseModel>;
});

