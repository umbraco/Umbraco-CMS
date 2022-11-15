/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { CallingConventions } from './CallingConventions';
import type { CustomAttributeData } from './CustomAttributeData';
import type { ICustomAttributeProvider } from './ICustomAttributeProvider';
import type { MemberTypes } from './MemberTypes';
import type { MethodAttributes } from './MethodAttributes';
import type { MethodImplAttributes } from './MethodImplAttributes';
import type { Module } from './Module';
import type { ParameterInfo } from './ParameterInfo';
import type { RuntimeMethodHandle } from './RuntimeMethodHandle';
import type { Type } from './Type';

export type MethodInfo = {
    readonly name?: string | null;
    declaringType?: Type;
    reflectedType?: Type;
    module?: Module;
    readonly customAttributes?: Array<CustomAttributeData> | null;
    readonly isCollectible?: boolean;
    readonly metadataToken?: number;
    attributes?: MethodAttributes;
    methodImplementationFlags?: MethodImplAttributes;
    callingConvention?: CallingConventions;
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
    methodHandle?: RuntimeMethodHandle;
    readonly isSecurityCritical?: boolean;
    readonly isSecuritySafeCritical?: boolean;
    readonly isSecurityTransparent?: boolean;
    memberType?: MemberTypes;
    returnParameter?: ParameterInfo;
    returnType?: Type;
    returnTypeCustomAttributes?: ICustomAttributeProvider;
};

