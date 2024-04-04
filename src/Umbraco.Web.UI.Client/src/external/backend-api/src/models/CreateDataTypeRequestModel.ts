/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DataTypePropertyPresentationModel } from './DataTypePropertyPresentationModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';

export type CreateDataTypeRequestModel = {
    name: string;
    editorAlias: string;
    editorUiAlias?: string | null;
    values: Array<DataTypePropertyPresentationModel>;
    id?: string | null;
    parent: ReferenceByIdModel;
};

