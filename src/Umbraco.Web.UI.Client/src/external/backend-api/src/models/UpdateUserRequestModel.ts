/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { UserPresentationBaseModel } from './UserPresentationBaseModel';

export type UpdateUserRequestModel = (UserPresentationBaseModel & {
    languageIsoCode?: string;
    contentStartNodeIds?: Array<string>;
    mediaStartNodeIds?: Array<string>;
});

