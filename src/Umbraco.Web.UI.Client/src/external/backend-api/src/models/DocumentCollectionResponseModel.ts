/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ContentCollectionResponseModelBaseDocumentValueModelDocumentVariantResponseModel } from './ContentCollectionResponseModelBaseDocumentValueModelDocumentVariantResponseModel';
import type { DocumentTypeCollectionReferenceResponseModel } from './DocumentTypeCollectionReferenceResponseModel';
export type DocumentCollectionResponseModel = (ContentCollectionResponseModelBaseDocumentValueModelDocumentVariantResponseModel & {
    documentType: DocumentTypeCollectionReferenceResponseModel;
    updater?: string | null;
});

