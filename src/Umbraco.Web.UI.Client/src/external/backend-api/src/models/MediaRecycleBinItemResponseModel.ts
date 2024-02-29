/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { MediaTypeReferenceResponseModel } from './MediaTypeReferenceResponseModel';
import type { RecycleBinItemResponseModelBaseModel } from './RecycleBinItemResponseModelBaseModel';
import type { VariantItemResponseModel } from './VariantItemResponseModel';
export type MediaRecycleBinItemResponseModel = (RecycleBinItemResponseModelBaseModel & {
    mediaType: MediaTypeReferenceResponseModel;
    variants: Array<VariantItemResponseModel>;
});

