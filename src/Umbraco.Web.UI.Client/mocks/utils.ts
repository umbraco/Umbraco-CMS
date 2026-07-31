export const arrayFilter = (filterBy: Array<string>, value?: Array<string>): boolean => {
	// if a filter is not set, return all items
	if (!filterBy) {
		return true;
	}

	return filterBy.some((filterValue: string) => value?.includes(filterValue));
};

export const objectArrayFilter = (filterBy: Array<any>, value: Array<any>, key: string): boolean => {
	if (!filterBy || !value) {
		return true;
	}

	return value.map((value) => value[key]).some((value: any) => filterBy.includes(value));
};

export const stringFilter = (filterBy: Array<string>, value?: string): boolean => {
	// if a filter is not set, return all items
	if (!filterBy || !value) {
		return true;
	}

	return filterBy.includes(value);
};

export const queryFilter = (filterBy: string, value?: string) => {
	if (!filterBy || !value) {
		return true;
	}

	const query = filterBy.toLowerCase();
	return value.toLowerCase().includes(query);
};

/**
 * Creates a problem details object.
 * @param {object} problemDetails The problem details object.
 * @param {string} problemDetails.title The title of the problem, which will be shown to the user.
 * @param {string} problemDetails.detail A human-readable explanation specific to this occurrence of the problem, which will be shown to the user.
 * @param {number} problemDetails.status The HTTP status code for this occurrence of the problem.
 * @param {string} problemDetails.type A URI reference that identifies the problem type.
 * @returns {object} The problem details object.
 */
export function createProblemDetails(problemDetails: {
	title: string;
	detail?: string;
	type?: string;
	status?: number;
}): object {
	return problemDetails;
}

/**
 * Applies the `skip` and `take` query parameters of a request to an already-collected paged response, so a mocked
 * endpoint pages like the real one does. `total` remains the full count.
 * @param {object} response The full set of items, with their total.
 * @param {Request} request The intercepted request to read `skip` and `take` from.
 * @returns {object} The requested page of items, with the unchanged total.
 */
export function pageResponse<ItemType>(
	response: { items: Array<ItemType>; total: number },
	request: Request,
): { items: Array<ItemType>; total: number } {
	const url = new URL(request.url);
	const skip = Number(url.searchParams.get('skip')) || 0;
	const take = Number(url.searchParams.get('take')) || 100;

	return { items: response.items.slice(skip, skip + take), total: response.total };
}
