import { DataSourceResponse } from '../index.js';
import { Diff } from '@umbraco-cms/backoffice/utils';

/**
 * This function extends the data set of a DataSourceResponse.
 * Provide the desired type as a generic type parameter.
 * This will require the appendData argument to fill in the missing properties of the data.
 * @param response {DataSourceResponse<object>}
 * @param appendData {object} Additional properties to append to the data.
 * @returns {DataSourceResponse<object>}
 *
 * @example
 *
 * type originalResponseType = { foo: string };
 * type extendedResponseType = { foo: string, bar: string };
 *
 * const extendedResponse = extendDataSourceResponseData<extendedResponseType>(originalResponse, { bar: 'some additional data' });
 *
 * extendedResponse.data.bar is now equal to 'some additional data'
 */
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
