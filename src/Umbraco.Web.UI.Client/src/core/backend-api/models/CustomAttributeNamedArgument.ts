/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { CustomAttributeTypedArgument } from './CustomAttributeTypedArgument';
import type { MemberInfo } from './MemberInfo';

export type CustomAttributeNamedArgument = {
    memberInfo?: MemberInfo;
    typedValue?: CustomAttributeTypedArgument;
    readonly memberName?: string | null;
    readonly isField?: boolean;
};

