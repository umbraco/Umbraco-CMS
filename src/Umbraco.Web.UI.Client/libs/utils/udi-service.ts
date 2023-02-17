export function buildUdi(entityType: string, guid: string) {
	return 'umb://' + entityType + '/' + guid.replace(/-/g, '');
}
