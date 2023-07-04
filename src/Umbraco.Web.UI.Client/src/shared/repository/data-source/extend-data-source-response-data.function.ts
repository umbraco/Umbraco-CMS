import { DataSourceResponse } from '../index.js';

type FilterKeys<T, U> = {
	[K in keyof T]: K extends keyof U ? never : K;
};

type Diff<U, T> = Pick<T, FilterKeys<T, U>[keyof T]>;

export function extendDataSourceResponseData<
	ExtendedDataType extends IncomingDataType,
	IncomingDataType extends object = object,
	MissingPropsType extends object = Omit<Diff<IncomingDataType, ExtendedDataType>, '$type'>,
	// Maybe this Omit<..., "$ype"> can be removed, but for now it kept showing up as a difference, though its not a difference on the two types.
	ToType = IncomingDataType & ExtendedDataType
>(response: DataSourceResponse<IncomingDataType>, appendData: MissingPropsType): DataSourceResponse<ToType> {
	if (response.data === undefined) return response as unknown as DataSourceResponse<ToType>;
	return { ...response, data: { ...response.data, ...appendData } as unknown as ToType };
}
