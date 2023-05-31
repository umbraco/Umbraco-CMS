export type MemoizationFunction<R> = (previousResult: R, currentResult: R) => boolean;
