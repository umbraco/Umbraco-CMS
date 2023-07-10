/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { PublicAccessBaseModel } from './PublicAccessBaseModel';

export type PublicAccessRequestModel = (PublicAccessBaseModel & {
    memberUserNames?: Array<string>;
    memberGroupNames?: Array<string>;
});

