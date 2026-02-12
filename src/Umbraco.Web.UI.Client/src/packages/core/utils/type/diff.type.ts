type _FilterKeys<T, U> = {
	[K in keyof T]: K extends keyof U ? never : K;
};

// eslint-disable-next-line @typescript-eslint/naming-convention
export type Diff<U, T> = Pick<T, _FilterKeys<T, U>[keyof T]>;
