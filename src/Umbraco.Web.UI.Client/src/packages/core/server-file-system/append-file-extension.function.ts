export const appendFileExtensionIfNeeded = (fileName: string, extension: string) => {
	if (!fileName.endsWith(extension)) {
		return fileName + extension;
	}

	return fileName;
};
