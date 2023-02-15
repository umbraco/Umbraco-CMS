/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { CallingConventionsModel } from './CallingConventionsModel';
import type { CustomAttributeDataModel } from './CustomAttributeDataModel';
import type { MemberTypesModel } from './MemberTypesModel';
import type { MethodAttributesModel } from './MethodAttributesModel';
import type { MethodImplAttributesModel } from './MethodImplAttributesModel';
import type { ModuleModel } from './ModuleModel';
import type { RuntimeMethodHandleModel } from './RuntimeMethodHandleModel';
import type { TypeModel } from './TypeModel';

export type ConstructorInfoModel = {
    readonly name?: string;
    declaringType?: TypeModel;
    reflectedType?: TypeModel;
    module?: ModuleModel;
    readonly customAttributes?: Array<CustomAttributeDataModel>;
    readonly isCollectible?: boolean;
    readonly metadataToken?: number;
    attributes?: MethodAttributesModel;
    methodImplementationFlags?: MethodImplAttributesModel;
    callingConvention?: CallingConventionsModel;
    readonly isAbstract?: boolean;
    readonly isConstructor?: boolean;
    readonly isFinal?: boolean;
    readonly isHideBySig?: boolean;
    readonly isSpecialName?: boolean;
    readonly isStatic?: boolean;
    readonly isVirtual?: boolean;
    readonly isAssembly?: boolean;
    readonly isFamily?: boolean;
    readonly isFamilyAndAssembly?: boolean;
    readonly isFamilyOrAssembly?: boolean;
    readonly isPrivate?: boolean;
    readonly isPublic?: boolean;
    readonly isConstructedGenericMethod?: boolean;
    readonly isGenericMethod?: boolean;
    readonly isGenericMethodDefinition?: boolean;
    readonly containsGenericParameters?: boolean;
    methodHandle?: RuntimeMethodHandleModel;
    readonly isSecurityCritical?: boolean;
    readonly isSecuritySafeCritical?: boolean;
    readonly isSecurityTransparent?: boolean;
    memberType?: MemberTypesModel;
};
