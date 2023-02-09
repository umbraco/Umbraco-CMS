/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { CustomAttributeDataModel } from './CustomAttributeDataModel';
import type { MemberTypesModel } from './MemberTypesModel';
import type { MethodInfoModel } from './MethodInfoModel';
import type { ModuleModel } from './ModuleModel';
import type { PropertyAttributesModel } from './PropertyAttributesModel';
import type { TypeModel } from './TypeModel';

export type PropertyInfoModel = {
    readonly name?: string;
    declaringType?: TypeModel;
    reflectedType?: TypeModel;
    module?: ModuleModel;
    readonly customAttributes?: Array<CustomAttributeDataModel>;
    readonly isCollectible?: boolean;
    readonly metadataToken?: number;
    memberType?: MemberTypesModel;
    propertyType?: TypeModel;
    attributes?: PropertyAttributesModel;
    readonly isSpecialName?: boolean;
    readonly canRead?: boolean;
    readonly canWrite?: boolean;
    getMethod?: MethodInfoModel;
    setMethod?: MethodInfoModel;
};

