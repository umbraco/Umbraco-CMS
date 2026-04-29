export const getParentPathFromServerPath = (serverPath: string): string | null => {
	const parentPath = serverPath.substring(0, serverPath.lastIndexOf('/'));
	return parentPath || null;
};
