/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ConstructorInfo } from './ConstructorInfo';
import type { CustomAttributeNamedArgument } from './CustomAttributeNamedArgument';
import type { CustomAttributeTypedArgument } from './CustomAttributeTypedArgument';
import type { Type } from './Type';

export type CustomAttributeData = {
    attributeType?: Type;
    constructor?: ConstructorInfo;
    readonly constructorArguments?: Array<CustomAttributeTypedArgument> | null;
    readonly namedArguments?: Array<CustomAttributeNamedArgument> | null;
};

