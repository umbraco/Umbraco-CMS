export function naiveObjectComparison(objOne: any, objTwo: any): boolean {
	return JSON.stringify(objOne) === JSON.stringify(objTwo);
}
