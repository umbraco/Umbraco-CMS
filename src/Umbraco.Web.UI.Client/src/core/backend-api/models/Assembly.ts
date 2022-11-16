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
    readonly definedTypes?: Array<TypeInfo> | null;
    readonly exportedTypes?: Array<Type> | null;
    /**
     * @deprecated
     */
    readonly codeBase?: string | null;
    entryPoint?: MethodInfo;
    readonly fullName?: string | null;
    readonly imageRuntimeVersion?: string | null;
    readonly isDynamic?: boolean;
    readonly location?: string | null;
    readonly reflectionOnly?: boolean;
    readonly isCollectible?: boolean;
    readonly isFullyTrusted?: boolean;
    readonly customAttributes?: Array<CustomAttributeData> | null;
    /**
     * @deprecated
     */
    readonly escapedCodeBase?: string | null;
    manifestModule?: Module;
    readonly modules?: Array<Module> | null;
    /**
     * @deprecated
     */
    readonly globalAssemblyCache?: boolean;
    readonly hostContext?: number;
    securityRuleSet?: SecurityRuleSet;
};

