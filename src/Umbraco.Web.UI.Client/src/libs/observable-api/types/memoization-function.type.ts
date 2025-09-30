// eslint-disable-next-line @typescript-eslint/naming-convention
export type MemoizationFunction<R> = (previousResult: R, currentResult: R) => boolean;
