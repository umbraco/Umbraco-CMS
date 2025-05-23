export interface UmbDataApiResponse<ResponseType extends { data: unknown } = { data: unknown }> {
	data: ResponseType['data'];
}
