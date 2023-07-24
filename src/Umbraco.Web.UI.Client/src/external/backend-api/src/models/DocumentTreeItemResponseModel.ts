/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentTreeItemResponseModel } from './ContentTreeItemResponseModel';
import type { VariantTreeItemModel } from './VariantTreeItemModel';

export type DocumentTreeItemResponseModel = (ContentTreeItemResponseModel & {
    isProtected?: boolean;
    isPublished?: boolean;
    isEdited?: boolean;
    contentTypeId?: string;
    variants?: Array<VariantTreeItemModel>;
    icon?: string;
});

