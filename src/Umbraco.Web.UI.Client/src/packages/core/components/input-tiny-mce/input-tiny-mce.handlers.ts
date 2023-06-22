import type { tinymce } from '@umbraco-cms/backoffice/external/tinymce';

export const pastePreProcessHandler: tinymce.RawEditorOptions['paste_preprocess'] = (_editor, args) => {
	// Remove spans
	args.content = args.content.replace(/<\s*span[^>]*>(.*?)<\s*\/\s*span>/g, '$1');
	// Convert b to strong.
	args.content = args.content.replace(/<\s*b([^>]*)>(.*?)<\s*\/\s*b([^>]*)>/g, '<strong$1>$2</strong$3>');
	// convert i to em
	args.content = args.content.replace(/<\s*i([^>]*)>(.*?)<\s*\/\s*i([^>]*)>/g, '<em$1>$2</em$3>');
};

export const uploadImageHandler: tinymce.RawEditorOptions['images_upload_handler'] = (blobInfo, progress) => {
	return new Promise((resolve, reject) => {
		const xhr = new XMLHttpRequest();
		xhr.open('POST', window.Umbraco?.Sys.ServerVariables.umbracoUrls.tinyMceApiBaseUrl + 'UploadImage');

		xhr.onloadstart = () =>
			document.dispatchEvent(new CustomEvent('rte.file.uploading', { composed: true, bubbles: true }));

		xhr.onloadend = () =>
			document.dispatchEvent(new CustomEvent('rte.file.uploaded', { composed: true, bubbles: true }));

		xhr.upload.onprogress = (e) => progress((e.loaded / e.total) * 100);

		xhr.onerror = () => reject('Image upload failed due to a XHR Transport error. Code: ' + xhr.status);

		xhr.onload = () => {
			if (xhr.status < 200 || xhr.status >= 300) {
				reject('HTTP Error: ' + xhr.status);
				return;
			}

			const json = JSON.parse(xhr.responseText);

			if (!json || typeof json.tmpLocation !== 'string') {
				reject('Invalid JSON: ' + xhr.responseText);
				return;
			}

			// Put temp location into localstorage (used to update the img with data-tmpimg later on)
			localStorage.set(`tinymce__${blobInfo.blobUri()}`, json.tmpLocation);

			// We set the img src url to be the same as we started
			// The Blob URI is stored in TinyMce's cache
			// so the img still shows in the editor
			resolve(blobInfo.blobUri());
		};

		const formData = new FormData();
		formData.append('file', blobInfo.blob(), blobInfo.blob().name);

		xhr.send(formData);
	});
};
