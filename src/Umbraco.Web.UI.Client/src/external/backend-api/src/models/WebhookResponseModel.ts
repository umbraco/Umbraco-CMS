/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { WebhookEventResponseModel } from './WebhookEventResponseModel';
import type { WebhookModelBaseModel } from './WebhookModelBaseModel';
export type WebhookResponseModel = (WebhookModelBaseModel & {
    id: string;
    events: Array<WebhookEventResponseModel>;
});

