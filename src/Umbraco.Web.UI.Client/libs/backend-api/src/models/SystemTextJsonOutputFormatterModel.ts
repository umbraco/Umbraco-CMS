/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { EncodingModel } from './EncodingModel';
import type { JsonSerializerOptionsModel } from './JsonSerializerOptionsModel';

export type SystemTextJsonOutputFormatterModel = {
    $type: string;
    readonly supportedMediaTypes?: Array<string>;
    readonly supportedEncodings?: Array<EncodingModel>;
    serializerOptions?: JsonSerializerOptionsModel;
};

