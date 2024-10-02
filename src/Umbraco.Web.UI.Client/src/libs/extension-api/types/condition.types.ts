import type { ManifestBase } from './manifest-base.interface.js';

export interface UmbConditionConfigBase<AliasType extends string = string> {
	alias: AliasType;
}

export type ConditionTypeMap<ConditionConfigs extends UmbConditionConfigBase> = {
	[Condition in ConditionConfigs as Condition['alias']]: Condition;
} & {
	[key: string]: UmbConditionConfigBase;
};

export type SpecificConditionTypeOrUmbConditionConfigBase<
	ConditionConfigs extends UmbConditionConfigBase,
	T extends keyof ConditionTypeMap<ConditionConfigs> | string,
> = T extends keyof ConditionTypeMap<ConditionConfigs> ? ConditionTypeMap<ConditionConfigs>[T] : UmbConditionConfigBase;

export interface ManifestWithDynamicConditions<ConditionConfigs extends UmbConditionConfigBase = UmbConditionConfigBase>
	extends ManifestBase {
	/**
	 * Set the conditions for when the extension should be loaded
	 */
	conditions?: Array<ConditionConfigs>;
	/**
	 * Define one or more extension aliases that this extension should overwrite.
	 */
	overwrites?: string | Array<string>;
}
