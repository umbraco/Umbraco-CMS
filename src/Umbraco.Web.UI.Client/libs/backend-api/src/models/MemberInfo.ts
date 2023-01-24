/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { CustomAttributeData } from './CustomAttributeData';
import type { MemberTypes } from './MemberTypes';
import type { Module } from './Module';
import type { Type } from './Type';

export type MemberInfo = {
    memberType?: MemberTypes;
    readonly name?: string;
    declaringType?: Type;
    reflectedType?: Type;
    module?: Module;
    readonly customAttributes?: Array<CustomAttributeData>;
    readonly isCollectible?: boolean;
    readonly metadataToken?: number;
};

