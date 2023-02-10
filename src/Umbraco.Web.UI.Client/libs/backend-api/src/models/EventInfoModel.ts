/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { CustomAttributeDataModel } from './CustomAttributeDataModel';
import type { EventAttributesModel } from './EventAttributesModel';
import type { MemberTypesModel } from './MemberTypesModel';
import type { MethodInfoModel } from './MethodInfoModel';
import type { ModuleModel } from './ModuleModel';
import type { TypeModel } from './TypeModel';

export type EventInfoModel = {
    readonly name?: string;
    declaringType?: TypeModel;
    reflectedType?: TypeModel;
    module?: ModuleModel;
    readonly customAttributes?: Array<CustomAttributeDataModel>;
    readonly isCollectible?: boolean;
    readonly metadataToken?: number;
    memberType?: MemberTypesModel;
    attributes?: EventAttributesModel;
    readonly isSpecialName?: boolean;
    addMethod?: MethodInfoModel;
    removeMethod?: MethodInfoModel;
    raiseMethod?: MethodInfoModel;
    readonly isMulticast?: boolean;
    eventHandlerType?: TypeModel;
};

