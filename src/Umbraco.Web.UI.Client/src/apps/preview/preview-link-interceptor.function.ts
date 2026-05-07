// Links to these resource types cannot be followed inside the sandboxed preview iframe
// (Chrome blocks downloads and inline PDF rendering from sandboxed contexts), so the
// preview shell intercepts them and opens them in a new top-level tab instead.
const NON_HTML_RESOURCE_PATTERN =
	/\.(pdf|doc|docx|xls|xlsx|ppt|pptx|csv|txt|rtf|zip|7z|rar|tar|gz|mp3|wav|ogg|opus|flac|aiff|mp4|mov|webm|ogv|mkv|avi|wmv)$/i;

/**
 * Attaches a click listener to the iframe document that intercepts links to resources
 * the sandbox would block (downloads, PDFs, archives, media) and opens them in a new
 * top-level tab via the un-sandboxed parent shell.
 */
export function attachPreviewLinkInterceptor(iframe: HTMLIFrameElement): void {
	const doc = iframe.contentDocument;
	if (!doc) return;

	doc.addEventListener('click', (event) => {
		if (event.defaultPrevented) return;

		const link = (event.target as Element | null)?.closest('a');
		if (!link?.href) return;

		const isDownload = link.hasAttribute('download');
		const isNonHtmlResource = NON_HTML_RESOURCE_PATTERN.test(new URL(link.href).pathname);
		if (!isDownload && !isNonHtmlResource) return;

		event.preventDefault();
		window.open(link.href, '_blank', 'noopener,noreferrer');
	});
}
