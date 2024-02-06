/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ReferenceByIdModel } from './ReferenceByIdModel';
import type { TreeItemPresentationModel } from './TreeItemPresentationModel';

export type EntityTreeItemResponseModel = (TreeItemPresentationModel & {
    id: string;
    parent?: ReferenceByIdModel | null;
});

