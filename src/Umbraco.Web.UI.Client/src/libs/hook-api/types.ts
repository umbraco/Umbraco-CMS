export type UmbHookMethod<ValueType, MetaType extends Record<string, unknown> = Record<string, unknown>> = (
	data: ValueType,
	meta: MetaType,
) => Promise<ValueType> | ValueType;

export interface UmbHookEntry<ValueType, MetaType extends Record<string, unknown> = Record<string, unknown>> {
	method: UmbHookMethod<ValueType, MetaType>;
	weight: number;
}

export type UmbParallelHookMethod<MetaType extends Record<string, unknown> = Record<string, unknown>> = (
	meta: MetaType,
) => Promise<void> | void;
