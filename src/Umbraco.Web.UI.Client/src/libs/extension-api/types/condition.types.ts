import type { ManifestBase } from "./manifest-base.interface.js";

export interface UmbConditionConfigBase<AliasType extends string = string> {
	alias: AliasType;
}

export type ConditionTypeMap<ConditionTypes extends UmbConditionConfigBase> = {
	[Condition in ConditionTypes as Condition['alias']]: Condition;
} & {
	[key: string]: UmbConditionConfigBase;
};

export type SpecificConditionTypeOrUmbConditionConfigBase<
	ConditionTypes extends UmbConditionConfigBase,
	T extends keyof ConditionTypeMap<ConditionTypes> | string
> = T extends keyof ConditionTypeMap<ConditionTypes> ? ConditionTypeMap<ConditionTypes>[T] : UmbConditionConfigBase;

export interface ManifestWithDynamicConditions<ConditionTypes extends UmbConditionConfigBase = UmbConditionConfigBase>
	extends ManifestBase {
	/**
	 * Set the conditions for when the extension should be loaded
	 */
	conditions?: Array<ConditionTypes>;
	/**
	 * Define one or more extension aliases that this extension should overwrite.
	 */
	overwrites?: string | Array<string>;
}