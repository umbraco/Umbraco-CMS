/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { CreateContentForDocumentRequestModel } from './CreateContentForDocumentRequestModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';

export type CreateDocumentRequestModel = (CreateContentForDocumentRequestModel & {
    documentType: ReferenceByIdModel;
    template?: ReferenceByIdModel | null;
});

