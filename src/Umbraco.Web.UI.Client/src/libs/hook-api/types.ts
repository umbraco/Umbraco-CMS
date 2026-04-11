export type UmbHookMethod<T> = (data: T) => Promise<T> | T;

export interface UmbHookEntry<T> {
	method: UmbHookMethod<T>;
	weight: number;
}
