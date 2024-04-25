// They are currently hardcoded on the backend. Go by GUIDs in case they get renamed..?
export enum UmbMediaTypeFileType {
	SVG = 'c4b1efcf-a9d5-41c4-9621-e9d273b52a9c', //Vector Graphics (SVG)
	IMAGE = 'cc07b313-0843-4aa8-bbda-871c8da728c8', //Image
	AUDIO = 'a5ddeee0-8fd8-4cee-a658-6f1fcdb00de3', //Audio
	VIDEO = 'f6c515bb-653c-4bdc-821c-987729ebe327', //Video
	ARTICLE = 'a43e3414-9599-4230-a7d3-943a21b20122', //Article
	FILE = '4c52d8ab-54e6-40cd-999c-7a5f24903e4d', //File
	FOLDER = 'f38bd2d7-65d0-48e6-95dc-87ce06ec2d3d', //Folder
}

export function getMediaTypeByFileExtension(extension: string) {
	if (!extension) return UmbMediaTypeFileType.FOLDER;
	if (extension === 'svg') return UmbMediaTypeFileType.SVG;
	if (['jpg', 'jpeg', 'gif', 'bmp', 'png', 'tiff', 'tif', 'webp'].includes(extension))
		return UmbMediaTypeFileType.IMAGE;
	if (['mp3', 'weba', 'oga', 'opus'].includes(extension)) return UmbMediaTypeFileType.AUDIO;
	if (['mp4', 'webm', 'ogv'].includes(extension)) return UmbMediaTypeFileType.VIDEO;
	if (['pdf', 'docx', 'doc'].includes(extension)) return UmbMediaTypeFileType.ARTICLE;
	return UmbMediaTypeFileType.FILE;
}

export function getMediaTypeByFileMimeType(mimetype: string) {
	if (!mimetype) return UmbMediaTypeFileType.FOLDER;
	if (mimetype === 'image/svg+xml') return UmbMediaTypeFileType.SVG;
	const [type, extension] = mimetype.split('/');
	if (type === 'image') return UmbMediaTypeFileType.IMAGE;
	if (type === 'audio') return UmbMediaTypeFileType.AUDIO;
	if (type === 'video') return UmbMediaTypeFileType.VIDEO;
	if (['pdf', 'docx', 'doc'].includes(extension)) return UmbMediaTypeFileType.ARTICLE;
	return UmbMediaTypeFileType.FILE;
}

export function isMediaTypeRenderable(mediaTypeUnique: string) {
	if (mediaTypeUnique === UmbMediaTypeFileType.IMAGE) return true;
	if (mediaTypeUnique === UmbMediaTypeFileType.SVG) return true;
	return false;
}

export function isMediaTypeFolder(mediaTypeUnique: string) {
	return mediaTypeUnique === UmbMediaTypeFileType.FOLDER;
}
