/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { WebhookModelBaseModel } from './WebhookModelBaseModel';

export type CreateWebhookRequestModel = (WebhookModelBaseModel & {
    id?: string | null;
    events: Array<string>;
});

