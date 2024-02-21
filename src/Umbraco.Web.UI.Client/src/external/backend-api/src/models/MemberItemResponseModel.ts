/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ItemResponseModelBaseModel } from './ItemResponseModelBaseModel';
import type { MemberTypeReferenceResponseModel } from './MemberTypeReferenceResponseModel';
import type { VariantItemResponseModel } from './VariantItemResponseModel';

export type MemberItemResponseModel = (ItemResponseModelBaseModel & {
    memberType: MemberTypeReferenceResponseModel;
    variants: Array<VariantItemResponseModel>;
});

