/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

export type CreateWebhookRequestModel = {
    enabled: boolean;
    url: string;
    contentTypeKeys: Array<string>;
    headers: Record<string, string>;
    id?: string | null;
    events: Array<string>;
};

