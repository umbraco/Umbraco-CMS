export type UmbSequentialHookMethod<ValueType, MetaType extends Record<string, unknown> = Record<string, unknown>> = (
	data: ValueType,
	meta: MetaType,
) => Promise<ValueType> | ValueType;

export interface UmbSequentialHookEntry<ValueType, MetaType extends Record<string, unknown> = Record<string, unknown>> {
	method: UmbSequentialHookMethod<ValueType, MetaType>;
	weight: number;
}

export type UmbParallelHookMethod<MetaType extends Record<string, unknown> = Record<string, unknown>> = (
	meta: MetaType,
) => Promise<void> | void;
