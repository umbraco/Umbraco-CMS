import type { UmbConditionConfigBase } from '../types/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export type UmbConditionControllerArguments<
	ConditionConfigType extends UmbConditionConfigBase = UmbConditionConfigBase,
	ConditionOnChangeCallbackType = () => void,
> = {
	host: UmbControllerHost;
	config: ConditionConfigType;
	onChange: ConditionOnChangeCallbackType;
};
