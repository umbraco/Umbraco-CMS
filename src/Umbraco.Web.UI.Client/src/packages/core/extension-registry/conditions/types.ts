import type { SectionAliasConditionConfig } from './section-alias.condition.js';
import type { SwitchConditionConfig } from './switch.condition.js';
import { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type ConditionTypes = SectionAliasConditionConfig | SwitchConditionConfig | UmbConditionConfigBase;
