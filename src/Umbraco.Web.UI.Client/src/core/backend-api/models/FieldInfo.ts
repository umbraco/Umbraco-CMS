/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { CustomAttributeData } from './CustomAttributeData';
import type { FieldAttributes } from './FieldAttributes';
import type { MemberTypes } from './MemberTypes';
import type { Module } from './Module';
import type { RuntimeFieldHandle } from './RuntimeFieldHandle';
import type { Type } from './Type';

export type FieldInfo = {
    readonly name?: string | null;
    declaringType?: Type;
    reflectedType?: Type;
    module?: Module;
    readonly customAttributes?: Array<CustomAttributeData> | null;
    readonly isCollectible?: boolean;
    readonly metadataToken?: number;
    memberType?: MemberTypes;
    attributes?: FieldAttributes;
    fieldType?: Type;
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
    fieldHandle?: RuntimeFieldHandle;
};

