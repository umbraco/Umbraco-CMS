/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ItemReferenceByIdResponseModel } from './ItemReferenceByIdResponseModel';
import type { MediaTypeReferenceResponseModel } from './MediaTypeReferenceResponseModel';
import type { VariantItemResponseModel } from './VariantItemResponseModel';

export type MediaRecycleBinItemResponseModel = {
    id: string;
    hasChildren: boolean;
    parent?: ItemReferenceByIdResponseModel | null;
    mediaType: MediaTypeReferenceResponseModel;
    variants: Array<VariantItemResponseModel>;
};

