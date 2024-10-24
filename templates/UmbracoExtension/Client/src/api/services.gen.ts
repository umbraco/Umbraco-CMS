// This file is auto-generated by @hey-api/openapi-ts

import { createClient, createConfig, type Options } from '@hey-api/client-fetch';
import type { GetUmbracoExampleApiV1PingError, GetUmbracoExampleApiV1PingResponse, GetUmbracoExampleApiV1WhatsMyNameError, GetUmbracoExampleApiV1WhatsMyNameResponse, GetUmbracoExampleApiV1WhatsTheTimeMrWolfError, GetUmbracoExampleApiV1WhatsTheTimeMrWolfResponse, GetUmbracoExampleApiV1WhoAmIError, GetUmbracoExampleApiV1WhoAmIResponse } from './types.gen';

export const client = createClient(createConfig());

export class ExamplesService {
    public static getUmbracoExampleApiV1Ping<ThrowOnError extends boolean = false>(options?: Options<unknown, ThrowOnError>) {
        return (options?.client ?? client).get<GetUmbracoExampleApiV1PingResponse, GetUmbracoExampleApiV1PingError, ThrowOnError>({
            ...options,
            url: '/umbraco/example/api/v1/Ping'
        });
    }
    
    public static getUmbracoExampleApiV1WhatsMyName<ThrowOnError extends boolean = false>(options?: Options<unknown, ThrowOnError>) {
        return (options?.client ?? client).get<GetUmbracoExampleApiV1WhatsMyNameResponse, GetUmbracoExampleApiV1WhatsMyNameError, ThrowOnError>({
            ...options,
            url: '/umbraco/example/api/v1/WhatsMyName'
        });
    }
    
    public static getUmbracoExampleApiV1WhatsTheTimeMrWolf<ThrowOnError extends boolean = false>(options?: Options<unknown, ThrowOnError>) {
        return (options?.client ?? client).get<GetUmbracoExampleApiV1WhatsTheTimeMrWolfResponse, GetUmbracoExampleApiV1WhatsTheTimeMrWolfError, ThrowOnError>({
            ...options,
            url: '/umbraco/example/api/v1/WhatsTheTimeMrWolf'
        });
    }
    
    public static getUmbracoExampleApiV1WhoAmI<ThrowOnError extends boolean = false>(options?: Options<unknown, ThrowOnError>) {
        return (options?.client ?? client).get<GetUmbracoExampleApiV1WhoAmIResponse, GetUmbracoExampleApiV1WhoAmIError, ThrowOnError>({
            ...options,
            url: '/umbraco/example/api/v1/WhoAmI'
        });
    }
    
}