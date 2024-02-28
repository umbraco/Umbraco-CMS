export function jsonStringComparison(objOne: any, objTwo: any): boolean {
	return JSON.stringify(objOne) === JSON.stringify(objTwo);
}
