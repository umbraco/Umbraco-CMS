import type { ProblemDetails } from "@umbraco-cms/backend-api";

export interface UmbDetailRepository<DetailType> {

	createDetailsScaffold(parentKey: string | null): Promise<{
		data?: DetailType;
		error?: ProblemDetails;
	}>

	requestDetails(key: string): Promise<{
		data?: DetailType;
		error?: ProblemDetails;
	}>

	create(data: DetailType): Promise<{
		error?: ProblemDetails;
	}>

	save(data: DetailType): Promise<{
		error?: ProblemDetails;
	}>

	delete(key: string): Promise<{
		error?: ProblemDetails;
	}>

}
