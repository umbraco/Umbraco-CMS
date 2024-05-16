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
