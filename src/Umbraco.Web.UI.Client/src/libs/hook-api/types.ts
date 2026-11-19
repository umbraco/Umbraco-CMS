/**
 * A method of a sequential hook. Receives the value returned by the previous method and returns the value for the next.
 */
export type UmbSequentialHookMethod<ValueType, MetaType> = (
	data: ValueType,
	meta: MetaType,
) => Promise<ValueType> | ValueType;

/**
 * A method registered on a sequential hook, with the weight deciding its place in the order.
 */
export interface UmbSequentialHookEntry<ValueType, MetaType> {
	method: UmbSequentialHookMethod<ValueType, MetaType>;
	/**
	 * Methods with a higher weight run first.
	 */
	weight: number;
}

/**
 * A method of a parallel hook. Runs at the same time as the other methods of the hook.
 */
export type UmbParallelHookMethod<MetaType> = (meta: MetaType) => Promise<void> | void;
