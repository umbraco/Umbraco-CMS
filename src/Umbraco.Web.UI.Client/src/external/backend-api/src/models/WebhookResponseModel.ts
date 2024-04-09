/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { WebhookEventResponseModel } from './WebhookEventResponseModel';

export type WebhookResponseModel = {
    enabled: boolean;
    url: string;
    contentTypeKeys: Array<string>;
    headers: Record<string, string>;
    id: string;
    events: Array<WebhookEventResponseModel>;
};

