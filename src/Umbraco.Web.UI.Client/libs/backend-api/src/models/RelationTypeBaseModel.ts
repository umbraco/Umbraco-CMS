/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

export type RelationTypeBaseModel = {
    name?: string;
    isBidirectional?: boolean;
    parentObjectType?: string | null;
    childObjectType?: string | null;
    isDependency?: boolean;
};
