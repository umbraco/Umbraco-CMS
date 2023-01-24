/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

export type Language = {
    id?: number;
    isoCode: string;
    name?: string | null;
    isDefault?: boolean;
    isMandatory?: boolean;
    fallbackLanguageId?: number | null;
};

