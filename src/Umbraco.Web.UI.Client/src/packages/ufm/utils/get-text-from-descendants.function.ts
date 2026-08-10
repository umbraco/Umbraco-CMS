/**
 * Recursively collects text content from a DOM subtree, descending into nested
 * shadow roots. Standard `Node.textContent` does not pierce shadow DOM boundaries,
 * which means resolved text inside child custom-elements (such as `<ufm-content-name>`
 * whose async result lives in its own shadow root) is invisible to it. This helper
 * walks the tree and reads through.
 * @param {ShadowRoot | Element | null | undefined} root - the element or shadow root to traverse. If an `Element` with a shadow root is provided, the shadow root is traversed in preference to the light DOM children.
 * @returns {string} the concatenated text content of every descendant text node, skipping empty fragments.
 */
export function getTextFromDescendants(root: ShadowRoot | Element | null | undefined): string {
	if (!root) return '';
	const target = root instanceof Element && root.shadowRoot ? root.shadowRoot : root;
	const parts: string[] = [];
	for (const node of target.childNodes) {
		if (node.nodeType === Node.TEXT_NODE) {
			parts.push(node.textContent ?? '');
		} else if (node.nodeType === Node.ELEMENT_NODE) {
			parts.push(getTextFromDescendants(node as Element));
		}
	}
	return parts.filter((x) => x).join('');
}
