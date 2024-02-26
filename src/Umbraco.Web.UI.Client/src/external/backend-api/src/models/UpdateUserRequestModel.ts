/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { UserPresentationBaseModel } from './UserPresentationBaseModel';

export type UpdateUserRequestModel = (UserPresentationBaseModel & {
    languageIsoCode: string;
    documentStartNodeIds: Array<string>;
    mediaStartNodeIds: Array<string>;
});

