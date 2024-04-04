/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { MediaTypeReferenceResponseModel } from './MediaTypeReferenceResponseModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';
import type { VariantItemResponseModel } from './VariantItemResponseModel';

export type MediaTreeItemResponseModel = {
    hasChildren: boolean;
    parent?: ReferenceByIdModel | null;
    noAccess: boolean;
    isTrashed: boolean;
    id: string;
    mediaType: MediaTypeReferenceResponseModel;
    variants: Array<VariantItemResponseModel>;
};

