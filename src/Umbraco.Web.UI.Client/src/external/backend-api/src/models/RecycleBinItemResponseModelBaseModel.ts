/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ItemReferenceByIdResponseModel } from './ItemReferenceByIdResponseModel';

export type RecycleBinItemResponseModelBaseModel = {
    id: string;
    type: string;
    hasChildren: boolean;
    parent?: ItemReferenceByIdResponseModel | null;
};

