export type UmbPartialSome<T, K extends keyof T> = Omit<T, K> & Partial<Pick<T, K>>;
