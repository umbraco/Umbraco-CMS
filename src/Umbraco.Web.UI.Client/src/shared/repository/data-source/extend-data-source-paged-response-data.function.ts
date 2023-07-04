import { DataSourceResponse, UmbPagedData } from '../index.js';
import { Diff } from '@umbraco-cms/backoffice/utils';

export function extendDataSourcePagedResponseData<
	ExtendedDataType extends IncomingDataType,
	IncomingDataType extends object = object,
	MissingPropsType extends object = Omit<Diff<IncomingDataType, ExtendedDataType>, '$type'>,
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
