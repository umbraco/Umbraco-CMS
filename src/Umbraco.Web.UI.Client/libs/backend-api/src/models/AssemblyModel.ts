/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { CustomAttributeDataModel } from './CustomAttributeDataModel';
import type { MethodInfoModel } from './MethodInfoModel';
import type { ModuleModel } from './ModuleModel';
import type { SecurityRuleSetModel } from './SecurityRuleSetModel';
import type { TypeInfoModel } from './TypeInfoModel';
import type { TypeModel } from './TypeModel';

export type AssemblyModel = {
    readonly definedTypes?: Array<TypeInfoModel>;
    readonly exportedTypes?: Array<TypeModel>;
    /**
     * @deprecated
     */
    readonly codeBase?: string | null;
    entryPoint?: MethodInfoModel;
    readonly fullName?: string | null;
    readonly imageRuntimeVersion?: string;
    readonly isDynamic?: boolean;
    readonly location?: string;
    readonly reflectionOnly?: boolean;
    readonly isCollectible?: boolean;
    readonly isFullyTrusted?: boolean;
    readonly customAttributes?: Array<CustomAttributeDataModel>;
    /**
     * @deprecated
     */
    readonly escapedCodeBase?: string;
    manifestModule?: ModuleModel;
    readonly modules?: Array<ModuleModel>;
    /**
     * @deprecated
     */
    readonly globalAssemblyCache?: boolean;
    readonly hostContext?: number;
    securityRuleSet?: SecurityRuleSetModel;
};

