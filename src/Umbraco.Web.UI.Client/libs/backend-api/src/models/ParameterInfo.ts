/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { CustomAttributeData } from './CustomAttributeData';
import type { MemberInfo } from './MemberInfo';
import type { ParameterAttributes } from './ParameterAttributes';
import type { Type } from './Type';

export type ParameterInfo = {
    attributes?: ParameterAttributes;
    member?: MemberInfo;
    readonly name?: string | null;
    parameterType?: Type;
    readonly position?: number;
    readonly isIn?: boolean;
    readonly isLcid?: boolean;
    readonly isOptional?: boolean;
    readonly isOut?: boolean;
    readonly isRetval?: boolean;
    readonly defaultValue?: any;
    readonly rawDefaultValue?: any;
    readonly hasDefaultValue?: boolean;
    readonly customAttributes?: Array<CustomAttributeData>;
    readonly metadataToken?: number;
};

