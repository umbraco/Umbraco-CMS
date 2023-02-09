/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { CustomAttributeDataModel } from './CustomAttributeDataModel';
import type { FieldAttributesModel } from './FieldAttributesModel';
import type { MemberTypesModel } from './MemberTypesModel';
import type { ModuleModel } from './ModuleModel';
import type { RuntimeFieldHandleModel } from './RuntimeFieldHandleModel';
import type { TypeModel } from './TypeModel';

export type FieldInfoModel = {
    readonly name?: string;
    declaringType?: TypeModel;
    reflectedType?: TypeModel;
    module?: ModuleModel;
    readonly customAttributes?: Array<CustomAttributeDataModel>;
    readonly isCollectible?: boolean;
    readonly metadataToken?: number;
    memberType?: MemberTypesModel;
    attributes?: FieldAttributesModel;
    fieldType?: TypeModel;
    readonly isInitOnly?: boolean;
    readonly isLiteral?: boolean;
    readonly isNotSerialized?: boolean;
    readonly isPinvokeImpl?: boolean;
    readonly isSpecialName?: boolean;
    readonly isStatic?: boolean;
    readonly isAssembly?: boolean;
    readonly isFamily?: boolean;
    readonly isFamilyAndAssembly?: boolean;
    readonly isFamilyOrAssembly?: boolean;
    readonly isPrivate?: boolean;
    readonly isPublic?: boolean;
    readonly isSecurityCritical?: boolean;
    readonly isSecuritySafeCritical?: boolean;
    readonly isSecurityTransparent?: boolean;
    fieldHandle?: RuntimeFieldHandleModel;
};

