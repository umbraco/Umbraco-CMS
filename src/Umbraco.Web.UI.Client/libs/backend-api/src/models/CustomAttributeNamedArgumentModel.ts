/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { CustomAttributeTypedArgumentModel } from './CustomAttributeTypedArgumentModel';
import type { MemberInfoModel } from './MemberInfoModel';

export type CustomAttributeNamedArgumentModel = {
    memberInfo?: MemberInfoModel;
    typedValue?: CustomAttributeTypedArgumentModel;
    readonly memberName?: string;
    readonly isField?: boolean;
};

