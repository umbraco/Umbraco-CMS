/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { CustomAttributeDataModel } from './CustomAttributeDataModel';
import type { MemberTypesModel } from './MemberTypesModel';
import type { ModuleModel } from './ModuleModel';
import type { TypeModel } from './TypeModel';

export type MemberInfoModel = {
    memberType?: MemberTypesModel;
    readonly name?: string;
    declaringType?: TypeModel;
    reflectedType?: TypeModel;
    module?: ModuleModel;
    readonly customAttributes?: Array<CustomAttributeDataModel>;
    readonly isCollectible?: boolean;
    readonly metadataToken?: number;
};

