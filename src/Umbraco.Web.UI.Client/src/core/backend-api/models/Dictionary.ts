/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { BackOfficeNotification } from './BackOfficeNotification';
import type { ContentApp } from './ContentApp';
import type { DictionaryTranslation } from './DictionaryTranslation';

export type Dictionary = {
    parentId?: string | null;
    translations?: Array<DictionaryTranslation> | null;
    contentApps?: Array<ContentApp> | null;
    readonly notifications?: Array<BackOfficeNotification> | null;
    name: string;
    key?: string;
    path?: string | null;
};

