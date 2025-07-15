/**
 * Triggers a client-side download of a file, using a `Blob` object.
 * To do this, a temporary anchor element is created, appended to the document body,
 * immediate triggered to download, then removed from the document body.
 * @param {any} data - The data to be downloaded.
 * @param {string} filename - The name of the file to be downloaded.
 * @param {string} mimeType - The MIME type of the file to be downloaded.
 * @returns {void}
 * @example
 * blobDownload(data, 'package.xml', 'text/xml');
 */
export const blobDownload = (data: any, filename: string, mimeType: string) => {
	const blob = new Blob([data], { type: mimeType });
	const url = window.URL.createObjectURL(blob);
	const a = document.createElement('a');
	a.href = url;
	a.download = filename;
	a.style.display = 'none';
	document.body.appendChild(a);
	a.dispatchEvent(new MouseEvent('click'));
	a.remove();
	window.URL.revokeObjectURL(url);
};
