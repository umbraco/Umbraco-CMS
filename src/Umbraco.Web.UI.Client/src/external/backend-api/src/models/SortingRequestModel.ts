/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ItemSortingRequestModel } from './ItemSortingRequestModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';
export type SortingRequestModel = {
    parent?: ReferenceByIdModel | null;
    sorting: Array<ItemSortingRequestModel>;
};

