/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ItemResponseModelBaseModel } from './ItemResponseModelBaseModel';
import type { MediaTypeReferenceResponseModel } from './MediaTypeReferenceResponseModel';
import type { VariantItemResponseModel } from './VariantItemResponseModel';
export type MediaItemResponseModel = (ItemResponseModelBaseModel & {
    isTrashed: boolean;
    mediaType: MediaTypeReferenceResponseModel;
    variants: Array<VariantItemResponseModel>;
});

