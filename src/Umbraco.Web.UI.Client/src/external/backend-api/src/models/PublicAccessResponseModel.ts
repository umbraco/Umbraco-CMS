/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { MemberGroupItemResponseModel } from './MemberGroupItemResponseModel';
import type { MemberItemResponseModel } from './MemberItemResponseModel';
import type { PublicAccessBaseModel } from './PublicAccessBaseModel';

export type PublicAccessResponseModel = (PublicAccessBaseModel & {
    members?: Array<MemberItemResponseModel>;
    groups?: Array<MemberGroupItemResponseModel>;
});

