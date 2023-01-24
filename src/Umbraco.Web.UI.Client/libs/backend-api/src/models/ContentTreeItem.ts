/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

export type ContentTreeItem = {
    name?: string;
    type?: string;
    icon?: string;
    hasChildren?: boolean;
    key?: string;
    isContainer?: boolean;
    parentKey?: string | null;
    noAccess?: boolean;
    isTrashed?: boolean;
};

