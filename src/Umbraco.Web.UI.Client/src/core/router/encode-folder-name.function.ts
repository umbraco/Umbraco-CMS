export const encodeFolderName = (path: string) =>
	encodeURIComponent(path.toLowerCase().replace(/\s+/g, '-'))
		//.replace(/-/g, '%2D')
		.replace(/_/g, '-')
		.replace(/\./g, '-')
		.replace(/!/g, '-')
		.replace(/~/g, '-')
		.replace(/\*/g, '-')
		.replace(/'/g, '')
		.replace(/\(/g, '-')
		.replace(/\)/g, '-');
