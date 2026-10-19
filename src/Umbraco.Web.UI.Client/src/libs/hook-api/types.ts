export type UmbSequentialHookMethod<ValueType, MetaType> = (
	data: ValueType,
	meta: MetaType,
) => Promise<ValueType> | ValueType;

export interface UmbSequentialHookEntry<ValueType, MetaType> {
	method: UmbSequentialHookMethod<ValueType, MetaType>;
	weight: number;
}

export type UmbParallelHookMethod<MetaType> = (meta: MetaType) => Promise<void> | void;
