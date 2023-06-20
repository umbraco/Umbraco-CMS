/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

export type PackageModelBaseModel = {
    name?: string;
    contentNodeId?: string | null;
    contentLoadChildNodes?: boolean;
    mediaIds?: Array<string>;
    mediaLoadChildNodes?: boolean;
    documentTypes?: Array<string>;
    mediaTypes?: Array<string>;
    dataTypes?: Array<string>;
    templates?: Array<string>;
    partialViews?: Array<string>;
    stylesheets?: Array<string>;
    scripts?: Array<string>;
    languages?: Array<string>;
    dictionaryItems?: Array<string>;
};
