/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { BackOfficeNotification } from './BackOfficeNotification';
import type { ContentApp } from './ContentApp';
import type { DictionaryTranslation } from './DictionaryTranslation';

export type Dictionary = {
    parentId?: string | null;
    translations?: Array<DictionaryTranslation>;
    contentApps?: Array<ContentApp>;
    readonly notifications?: Array<BackOfficeNotification>;
    name: string;
    key?: string;
    path?: string;
};

