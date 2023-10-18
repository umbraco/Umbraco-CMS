import { DataSourceResponse, UmbPagedData } from '../index.js';
import { Diff } from '@umbraco-cms/backoffice/utils';

/**
 * This function extends the data set of a paged DataSourceResponse.
 * Provide the desired type as a generic type parameter.
 * This will require the appendData argument to fill in the missing properties of the items.
 * @param response {DataSourceResponse<UmbPagedData<object>>}
 * @param appendData {object} Additional properties to append to the data set.
 * @returns {DataSourceResponse<UmbPagedData<object>>}
 *
 * @example
 *
 * type originalResponseType = { foo: string };
 * type extendedResponseType = { foo: string, bar: string };
 *
 * const extendedResponse = extendDataSourcePagedResponseData<extendedResponseType>(originalResponse, { bar: 'some additional data' });
 *
 * extendedResponse.data.items[0].bar is now equal to 'some additional data'
 */
export function extendDataSourcePagedResponseData<
	ExtendedDataType extends IncomingDataType,
	IncomingDataType extends object = object,
	MissingPropsType extends object = Diff<IncomingDataType, ExtendedDataType>,
	// Maybe this Omit<..., "$ype"> can be removed, but for now it kept showing up as a difference, though its not a difference on the two types.
	ToType = IncomingDataType & ExtendedDataType
>(
	response: DataSourceResponse<UmbPagedData<IncomingDataType>>,
	appendData: MissingPropsType
): DataSourceResponse<UmbPagedData<ToType>> {
	if (response.data === undefined) return response as unknown as DataSourceResponse<UmbPagedData<ToType>>;
	return {
		...response,
		data: {
			...response.data,
			items: response.data.items.map((x) => {
				return { ...x, ...appendData } as unknown as ToType;
			}),
		},
	};
}
