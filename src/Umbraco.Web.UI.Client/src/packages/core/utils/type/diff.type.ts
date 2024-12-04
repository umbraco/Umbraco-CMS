type _FilterKeys<T, U> = {
	[K in keyof T]: K extends keyof U ? never : K;
};

export type Diff<U, T> = Pick<T, _FilterKeys<T, U>[keyof T]>;
