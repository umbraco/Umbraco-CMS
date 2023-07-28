import type { UmbConditionConfigBase } from '../types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export type UmbConditionControllerArguments<
	ConditionConfigType extends UmbConditionConfigBase = UmbConditionConfigBase
> = { host: UmbControllerHost; config: ConditionConfigType; onChange: () => void };
