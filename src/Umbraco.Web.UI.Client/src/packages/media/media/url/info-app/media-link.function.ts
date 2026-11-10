/**
 * Returns the URL if it can be offered as a link, i.e. it resolves to an http or https location.
 * @param {string | undefined} url - The media URL.
 * @param {string} base - The base used to resolve relative URLs.
 * @returns {string | undefined} The URL, or undefined if it must not be rendered as a link.
 */
export function getLinkableMediaUrl(url: string | undefined, base: string = window.location.href): string | undefined {
	if (!url) return undefined;
	try {
		const { protocol } = new URL(url, base);
		return protocol === 'http:' || protocol === 'https:' ? url : undefined;
	} catch {
		return undefined;
	}
}

/**
 * Renders an image preview on a checkerboard background into the given document.
 * The image is referenced through an `<img>` element, so scripts embedded in an SVG do not run.
 * @param {Document} document - The document to render into.
 * @param {string} imageUrl - The URL of the image.
 */
export function renderImagePreview(document: Document, imageUrl: string): void {
	const body = document.body ?? document.documentElement.appendChild(document.createElement('body'));
	body.style.backgroundImage =
		'linear-gradient(45deg, #ccc 25%, transparent 25%), linear-gradient(135deg, #ccc 25%, transparent 25%), linear-gradient(45deg, transparent 75%, #ccc 75%), linear-gradient(135deg, transparent 75%, #ccc 75%)';
	body.style.backgroundSize = '30px 30px';
	body.style.backgroundPosition = '0 0, 15px 0, 15px -15px, 0px 15px';

	const image = document.createElement('img');
	image.src = imageUrl;
	body.replaceChildren(image);
}
