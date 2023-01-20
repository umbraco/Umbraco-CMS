/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { CustomAttributeData } from './CustomAttributeData';
import type { MethodInfo } from './MethodInfo';
import type { Module } from './Module';
import type { SecurityRuleSet } from './SecurityRuleSet';
import type { Type } from './Type';
import type { TypeInfo } from './TypeInfo';

export type Assembly = {
    readonly definedTypes?: Array<TypeInfo>;
    readonly exportedTypes?: Array<Type>;
    /**
     * @deprecated
     */
    readonly codeBase?: string | null;
    entryPoint?: MethodInfo;
    readonly fullName?: string | null;
    readonly imageRuntimeVersion?: string;
    readonly isDynamic?: boolean;
    readonly location?: string;
    readonly reflectionOnly?: boolean;
    readonly isCollectible?: boolean;
    readonly isFullyTrusted?: boolean;
    readonly customAttributes?: Array<CustomAttributeData>;
    /**
     * @deprecated
     */
    readonly escapedCodeBase?: string;
    manifestModule?: Module;
    readonly modules?: Array<Module>;
    /**
     * @deprecated
     */
    readonly globalAssemblyCache?: boolean;
    readonly hostContext?: number;
    securityRuleSet?: SecurityRuleSet;
};

