/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { CustomAttributeDataModel } from './CustomAttributeDataModel';
import type { MemberInfoModel } from './MemberInfoModel';
import type { ParameterAttributesModel } from './ParameterAttributesModel';
import type { TypeModel } from './TypeModel';

export type ParameterInfoModel = {
    attributes?: ParameterAttributesModel;
    member?: MemberInfoModel;
    readonly name?: string | null;
    parameterType?: TypeModel;
    readonly position?: number;
    readonly isIn?: boolean;
    readonly isLcid?: boolean;
    readonly isOptional?: boolean;
    readonly isOut?: boolean;
    readonly isRetval?: boolean;
    readonly defaultValue?: any;
    readonly rawDefaultValue?: any;
    readonly hasDefaultValue?: boolean;
    readonly customAttributes?: Array<CustomAttributeDataModel>;
    readonly metadataToken?: number;
};

