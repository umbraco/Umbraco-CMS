/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { CustomAttributeData } from './CustomAttributeData';
import type { MemberTypes } from './MemberTypes';
import type { MethodInfo } from './MethodInfo';
import type { Module } from './Module';
import type { PropertyAttributes } from './PropertyAttributes';
import type { Type } from './Type';

export type PropertyInfo = {
    readonly name?: string | null;
    declaringType?: Type;
    reflectedType?: Type;
    module?: Module;
    readonly customAttributes?: Array<CustomAttributeData> | null;
    readonly isCollectible?: boolean;
    readonly metadataToken?: number;
    memberType?: MemberTypes;
    propertyType?: Type;
    attributes?: PropertyAttributes;
    readonly isSpecialName?: boolean;
    readonly canRead?: boolean;
    readonly canWrite?: boolean;
    getMethod?: MethodInfo;
    setMethod?: MethodInfo;
};

