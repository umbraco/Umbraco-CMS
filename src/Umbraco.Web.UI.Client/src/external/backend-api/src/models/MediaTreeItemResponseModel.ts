/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentTreeItemResponseModel } from './ContentTreeItemResponseModel';
import type { MediaTypeReferenceResponseModel } from './MediaTypeReferenceResponseModel';
import type { VariantItemResponseModel } from './VariantItemResponseModel';

export type MediaTreeItemResponseModel = (ContentTreeItemResponseModel & {
    mediaType: MediaTypeReferenceResponseModel;
    variants: Array<VariantItemResponseModel>;
});

