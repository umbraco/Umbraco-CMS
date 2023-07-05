type FilterKeys<T, U> = {
	[K in keyof T]: K extends keyof U ? never : K;
};

export type Diff<U, T> = Pick<T, FilterKeys<T, U>[keyof T]>;
