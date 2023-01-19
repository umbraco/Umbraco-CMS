/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { CustomAttributeData } from './CustomAttributeData';
import type { EventAttributes } from './EventAttributes';
import type { MemberTypes } from './MemberTypes';
import type { MethodInfo } from './MethodInfo';
import type { Module } from './Module';
import type { Type } from './Type';

export type EventInfo = {
    readonly name?: string;
    declaringType?: Type;
    reflectedType?: Type;
    module?: Module;
    readonly customAttributes?: Array<CustomAttributeData>;
    readonly isCollectible?: boolean;
    readonly metadataToken?: number;
    memberType?: MemberTypes;
    attributes?: EventAttributes;
    readonly isSpecialName?: boolean;
    addMethod?: MethodInfo;
    removeMethod?: MethodInfo;
    raiseMethod?: MethodInfo;
    readonly isMulticast?: boolean;
    eventHandlerType?: Type;
};

