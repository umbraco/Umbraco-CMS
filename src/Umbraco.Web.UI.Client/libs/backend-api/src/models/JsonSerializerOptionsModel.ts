/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { JavaScriptEncoderModel } from './JavaScriptEncoderModel';
import type { JsonCommentHandlingModel } from './JsonCommentHandlingModel';
import type { JsonIgnoreConditionModel } from './JsonIgnoreConditionModel';
import type { JsonNamingPolicyModel } from './JsonNamingPolicyModel';
import type { JsonNumberHandlingModel } from './JsonNumberHandlingModel';
import type { JsonObjectConverterModel } from './JsonObjectConverterModel';
import type { JsonUnknownTypeHandlingModel } from './JsonUnknownTypeHandlingModel';
import type { ReferenceHandlerModel } from './ReferenceHandlerModel';
import type { UmbracoJsonTypeInfoResolverModel } from './UmbracoJsonTypeInfoResolverModel';

export type JsonSerializerOptionsModel = {
    readonly converters?: Array<JsonObjectConverterModel>;
    typeInfoResolver?: UmbracoJsonTypeInfoResolverModel | null;
    allowTrailingCommas?: boolean;
    defaultBufferSize?: number;
    encoder?: JavaScriptEncoderModel;
    dictionaryKeyPolicy?: JsonNamingPolicyModel;
    /**
     * @deprecated
     */
    ignoreNullValues?: boolean;
    defaultIgnoreCondition?: JsonIgnoreConditionModel;
    numberHandling?: JsonNumberHandlingModel;
    ignoreReadOnlyProperties?: boolean;
    ignoreReadOnlyFields?: boolean;
    includeFields?: boolean;
    maxDepth?: number;
    propertyNamingPolicy?: JsonNamingPolicyModel;
    propertyNameCaseInsensitive?: boolean;
    readCommentHandling?: JsonCommentHandlingModel;
    unknownTypeHandling?: JsonUnknownTypeHandlingModel;
    writeIndented?: boolean;
    referenceHandler?: ReferenceHandlerModel;
};

