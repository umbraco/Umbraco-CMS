export enum UmbMediaTypeFileType {
	SVG = 'Vector Graphics (SVG)',
	IMAGE = 'Image',
	AUDIO = 'Audio',
	VIDEO = 'Video',
	ARTICLE = 'Article',
	FILE = 'File',
}

export function getMediaTypeByFileExtension(extension: string) {
	if (extension === 'svg') return UmbMediaTypeFileType.SVG;
	if (['jpg', 'jpeg', 'gif', 'bmp', 'png', 'tiff', 'tif', 'webp'].includes(extension))
		return UmbMediaTypeFileType.IMAGE;
	if (['mp3', 'weba', 'oga', 'opus'].includes(extension)) return UmbMediaTypeFileType.AUDIO;
	if (['mp4', 'webm', 'ogv'].includes(extension)) return UmbMediaTypeFileType.VIDEO;
	if (['pdf', 'docx', 'doc'].includes(extension)) return UmbMediaTypeFileType.ARTICLE;
	return UmbMediaTypeFileType.FILE;
}

export function getMediaTypeByFileMimeType(mimetype: string) {
	if (mimetype === 'image/svg+xml') return UmbMediaTypeFileType.SVG;
	const [type, extension] = mimetype.split('/');
	if (type === 'image') return UmbMediaTypeFileType.IMAGE;
	if (type === 'audio') return UmbMediaTypeFileType.AUDIO;
	if (type === 'video') return UmbMediaTypeFileType.VIDEO;
	if (['pdf', 'docx', 'doc'].includes(extension)) return UmbMediaTypeFileType.ARTICLE;
	return UmbMediaTypeFileType.FILE;
}
